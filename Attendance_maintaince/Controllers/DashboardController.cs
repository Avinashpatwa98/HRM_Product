using Attendance_maintaince.Models;
using Microsoft.AspNetCore.Mvc;

namespace Attendance_maintaince.Controllers
{
    public class DashboardController : Controller
    {
        // GET: /Dashboard
        public IActionResult Index(string token)
        {
            // Check if the token is not null or empty
            if (!string.IsNullOrWhiteSpace(token))
            {
                
                HttpContext.Session.SetString("JwtToken", token);
            }

           
            var model = new Dashboard
            {
                
            };

            // Return the Dashboard view with the model
            return View("Dashboard", model); // This returns Dashboard.cshtml
        } 

    }
}
