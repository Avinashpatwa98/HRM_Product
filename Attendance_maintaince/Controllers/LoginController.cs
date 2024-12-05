using Attendance_maintaince.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Attendance_maintaince.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;

        public LoginController(IMemoryCache memoryCache, IConfiguration configuration, AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _memoryCache = memoryCache;
        }
        public IActionResult Login()
        {
            return View();
        }
        // Login Action
        [HttpPost]
        [EnableRateLimiting("loginPolicy")]
        public IActionResult Login(Login model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid login data." });
            }

            var user = _dbContext.Employees.FirstOrDefault(x => x.Email == model.Email);

            if (user != null && VerifyPassword(user.Password, model.Password))
            {
                var token = GenerateToken(user);

                ViewBag.Token = token;

                HttpContext.Response.Cookies.Append("AuthToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true
                });
                return RedirectToAction("Index", "Dashboard");
            }
            return Unauthorized(new { Message = "Invalid email or password." });
        }

        // Separated method to generate the JWT token
        private string GenerateToken(Employee user)
        {
            var claims = new[]
            {
               new Claim(ClaimTypes.Email, user.Email),
               new Claim(ClaimTypes.Name, user.Name),
               new Claim("EmployeeId", user.EmployeeId.ToString()),  // Ensure the EmployeeId claim is here
               new Claim("EmployeeName", user.Name.ToString()),
               new Claim(ClaimTypes.Role, user.Role), // Add the role claim here
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
           
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expiresInMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"]);
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private bool VerifyPassword(string hashedPassword, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        private bool IsValidUser(string email, string password)
        {
            var user = _dbContext.Employees.FirstOrDefault(x => x.Email == email);
            if (user != null)
            {
                return VerifyPassword(user.Password, password);
            }
            return false;
        }

        [HttpPost("Login/Registration")]
        public IActionResult Registration(Employee model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password) ||
                string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                return BadRequest("All fields (Email, Password, Name, and Phone Number) are required.");
            }

            // Check if user already exists
            if (_dbContext.Employees.Any(x => x.Email == model.Email))
            {
                return Conflict("User already exists.");
            }

            // Hash the password (implement this method)
            var hashedPassword = HashPassword(model.Password);
            var newUser = new Employee
            {
                Name = model.Name,
                Email = model.Email,
                Password = hashedPassword,
                PhoneNumber = model.PhoneNumber,
                Role = "Employee",
                //Role = "Admin",
                CreatedAt = DateTime.Now,

                Position = model.Position == "Custom" ? model.CustomPosition : model.Position
            };

            // Add the new user to the database
            _dbContext.Employees.Add(newUser);
            _dbContext.SaveChanges();

            // Redirect to login after successful registration
            return RedirectToAction("Login");
        }
        // GET: Registration
        [HttpGet("Login/Registration")]
        public IActionResult Registration()
        {
            return View(new Employee());
        }

        // Get user details
        [HttpGet("GetUser/{id}")]
        public IActionResult GetUser(Guid id)
        {
            var user = _dbContext.Employees.FirstOrDefault(x => x.EmployeeId == id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);
        }

        // Hash password using BCrypt
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        


      

    }
}
