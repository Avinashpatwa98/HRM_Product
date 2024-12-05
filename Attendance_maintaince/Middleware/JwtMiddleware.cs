using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Attendance_maintaince.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;

        public JwtMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _jwtKey = configuration["Jwt:Key"];
            _jwtIssuer = configuration["Jwt:Issuer"];
            _jwtAudience = configuration["Jwt:Audience"];

        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Extract token from header or cookie
            string token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                token = context.Request.Cookies["AuthToken"]; // Fallback to cookie if necessary
            }

            if (!string.IsNullOrEmpty(token))
            {
                
                try
                {
                    // Validate the token
                    var claimsPrincipal = ValidateToken(token);

                    // Attach claims to the current context
                    context.User = claimsPrincipal;
                }
                catch (Exception ex)
                {
                    // Token is invalid
                    Console.WriteLine($"Token validation failed: {ex.Message}");
                    // Optionally redirect to login or handle the error
                }
            }

            await _next(context); // Pass to the next middleware
        }
        #region Helper
        private ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = _jwtIssuer,
                ValidAudience = _jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
    ,
                ClockSkew = TimeSpan.Zero // Remove default 5-minute clock skew
            };

            // Validate the token
            var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return claimsPrincipal;
        }
        #endregion
    }
}

