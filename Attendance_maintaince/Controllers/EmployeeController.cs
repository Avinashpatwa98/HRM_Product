using Attendance_maintaince.Middleware;
using Attendance_maintaince.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Security.Claims;

namespace Attendance_maintaince.Controllers
{

    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult ViewProfile()
        {
            // Fetch employee data based on the authenticated user
            var employeeId = GetEmployeeIdFromClaims();

            string userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole != "Employee" && userRole != "Admin")  // Corrected condition
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            if (employeeId == Guid.Empty)
            {
                return BadRequest("Invalid Employee ID.");
            }


            // Fetch employee details by ID
            var employeeData = GetEmployeeById(employeeId);

            if (employeeData == null)
            {
                return NotFound("Employee data not found.");
            }

            // Pass the employee data to the view
            return View("ViewProfile", employeeData);

        }

        [HttpGet]
        public IActionResult ViewProfile1(Guid employeeId)
        {


            // Fetch employee details by ID
            var employeeData = GetEmployeeById(employeeId);

            if (employeeData == null)
            {
                return NotFound("Employee data not found.");
            }

            // Pass the employee data to the view
            return View("ViewProfile", employeeData);

        }

        [HttpGet]
        public IActionResult GetAttendance()
        {
            // Fetch employee data based on the authenticated user
            var employeeId = GetEmployeeIdFromClaims();

            // Fetch user role from claims
            string userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Verify if the user has the correct role
            if (userRole != "Employee" && userRole != "Admin")
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            if (employeeId == Guid.Empty)
            {
                return BadRequest("Invalid Employee ID.");
            }

            // Fetch attendance details by employee ID
            var attendanceData = GetAttendanceByEmployeeId(employeeId);

            if (attendanceData == null || !attendanceData.Any())
            {
                return NotFound("Attendance data not found.");
            }

            // Pass the attendance data to the view
            return View("AttendancePage", attendanceData);
        }

        // Method to fetch attendance records for the employee
        private IEnumerable<Attendance> GetAttendanceByEmployeeId(Guid employeeId)
        {
            return _context.Attendances.Where(e => e.EmployeeID == employeeId).ToList();
        }



        private Guid GetEmployeeIdFromClaims()
        {
            // Check if the user is authenticated and claims are available
            if (User?.Identity?.IsAuthenticated == true)
            {
                var claim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeId");
                if (claim != null)
                {
                    // Log or inspect the claim value
                    Console.WriteLine($"EmployeeId claim found: {claim.Value}");
                    return Guid.Parse(claim.Value);
                }
            }

            // Log a warning if the claim is not found
            Console.WriteLine("EmployeeId claim not found or user not authenticated.");
            return Guid.Empty;
        }


        private Employee GetEmployeeById(Guid employeeId)
        {
            return _context.Employees.FirstOrDefault(e => e.EmployeeId == employeeId && e.Status == "Active");
        }


        // POST: Employee/Delete
        [HttpPost]
        public IActionResult Delete(Guid employeeId)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.EmployeeId == employeeId && e.Status == "Active");

            if (employee == null)
            {
                return NotFound("Employee not found or already deleted.");
            }

            // Update the employee's status to 'Deleted'
            employee.Status = "Deleted";
            _context.SaveChanges();

            // Redirect back to the same employee profile page
            return RedirectToAction("ViewAllEmployees");
        }



        // GET: Employee/AddAttendance
        public IActionResult AddAttendance()
        {
            return View();
        }

        // POST: Employee/AddAttendance
        [HttpPost]
        public async Task<IActionResult> AddAttendance(Attendance attendance)
        {
            if (ModelState.IsValid)
            {
                attendance.ComingTime = DateTime.Now; // Set the current date
                attendance.DateOfDay = DateTime.Now.DayOfWeek.ToString();
                //  attendance.EmployeeId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)); 

                _context.Attendances.Add(attendance);
                await _context.SaveChangesAsync();

                return RedirectToAction("GetAttendance");
            }
            return View(attendance);
        }

        public IActionResult ViewAllEmployees()
        {
            var employees = GetAllEmployee();
            return View(employees);
        }

        public List<Employee> GetAllEmployee()
        {
            return _context.Employees.Where(e => e.Status == "Active" && e.Role == "Employee").ToList();
        }


        // POST method to process the bulk employee upload
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult AddBulkEmployees(IFormFile employeeFile)
        {
            if (employeeFile == null || employeeFile.Length == 0)
            {
                ModelState.AddModelError("EmployeeFile", "Please upload a valid file.");
                return View();
            }

            if (!employeeFile.FileName.EndsWith(".xlsx") && !employeeFile.FileName.EndsWith(".xls"))
            {
                ModelState.AddModelError("EmployeeFile", "Only Excel files are allowed.");
                return View();
            }

            // Process the Excel file and add employees
            var employees = ProcessExcelFile(employeeFile);
            if (employees == null || !employees.Any())
            {
                ModelState.AddModelError("EmployeeFile", "No valid employees were found in the file.");
                return View();
            }

            // Add the employees to the database
            _context.Employees.AddRange(employees);
            _context.SaveChanges();

            return RedirectToAction("ViewAllEmployees"); // Redirect to employee list after adding
        }

        // Method to process the Excel file and extract employee data
        private List<Employee> ProcessExcelFile(IFormFile file)
        {
            var employees = new List<Employee>();

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                using (var package = new ExcelPackage(stream))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    var worksheet = package.Workbook.Worksheets[0]; // Get the first sheet
                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++) // Skip header row
                    {
                        var employee = new Employee
                        {
                            EmployeeId = Guid.NewGuid(), // Generate new GUID for the employee
                            Name = worksheet.Cells[row, 1].Text,
                            Email = worksheet.Cells[row, 2].Text,
                            Password = BCrypt.Net.BCrypt.HashPassword(worksheet.Cells[row, 3].Text),  // Hash the password
                            Position= worksheet.Cells[row, 4].Text,
                            Role = "Employee",
                            Status = "Active",  // Default status
                            CreatedAt = DateTime.UtcNow
                        };

                        employees.Add(employee);
                    }
                }
            }

            return employees;
        }
    }
}












    

