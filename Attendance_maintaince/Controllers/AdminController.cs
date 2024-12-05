using Microsoft.AspNetCore.Mvc;
using Attendance_maintaince.Models;
using Microsoft.AspNetCore.Authorization;

namespace Attendance_maintaince.Controllers
{
   

    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/AddEmployee
        public IActionResult AddEmployee()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee(Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction("ViewEmployees");
            }
            return View(employee);
        }

        // GET: Admin/ViewEmployees
        public IActionResult ViewEmployees()
        {
            var employees = _context.Employees.ToList();
            return View(employees);
        }

        // GET: Admin/BulkAttendance
        public IActionResult BulkAttendance()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BulkAttendance(List<Attendance> attendances)
        {
            if (ModelState.IsValid)
            {
                _context.Attendances.AddRange(attendances);
                await _context.SaveChangesAsync();
                return RedirectToAction("ViewAttendance");
            }
            return View(attendances);
        }

        // GET: Admin/ViewAttendance
      /*  public IActionResult ViewAttendance()
        {
            var attendanceRecords = _context.Attendances.IncludeWith(a => a.Employee).ToList();

           // var attendanceRecords = _context.Attendances.(a => a.Employee).ToList();
            return View(attendanceRecords);
        }*/
    }

}
