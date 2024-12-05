using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;

namespace Attendance_maintaince.Middleware
{
    public class JwtAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var memoryCache = context.HttpContext.RequestServices.GetService<IMemoryCache>();
            var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();
            var dbContext = context.HttpContext.RequestServices.GetService<AppDbContext>();
            var request = context.HttpContext.Request;
            var secretKey = configuration["Jwt:Key"];
            var authHeader = request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(secretKey);

                try
                {
                    // Validate the token
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false, // Set to true if you want to validate issuer
                        ValidateAudience = false, // Set to true if you want to validate audience
                        ClockSkew = TimeSpan.Zero // No tolerance for token expiration
                    }, out SecurityToken validatedToken);

                    // Extract claims from the validated token
                    var jwtToken = validatedToken as JwtSecurityToken;

                    if (jwtToken != null)
                    {
                        var employeeIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "EmployeeId")?.Value;
                        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value; // 'sub' is often used for user ID

                        // Store claims in HttpContext.Items for later use
                        if (!string.IsNullOrEmpty(employeeIdClaim))
                        {
                            context.HttpContext.Items["EmployeeId"] = employeeIdClaim;
                        }

                        if (!string.IsNullOrEmpty(userIdClaim))
                        {
                            context.HttpContext.Items["UserId"] = userIdClaim;
                        }

                        // If token is valid, allow the request to proceed
                        base.OnActionExecuting(context);
                        return;
                    }

                    // If JWT token is not well-formed
                    context.Result = new UnauthorizedResult();
                }
                catch (SecurityTokenExpiredException)
                {
                    context.Result = new UnauthorizedResult(); // Token has expired
                }
                catch (SecurityTokenException)
                {
                    context.Result = new UnauthorizedResult(); // Invalid token
                }
                catch (Exception ex)
                {
                    // General exception handler for unexpected issues
                    context.Result = new UnauthorizedResult();
                }
            }
            else
            {
                // Missing or invalid Authorization header
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
