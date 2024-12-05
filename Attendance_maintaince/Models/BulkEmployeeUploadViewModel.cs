using System.ComponentModel.DataAnnotations;

namespace Attendance_maintaince.Models
{
    public class BulkEmployeeUploadViewModel
    {
        [Required]
        public IFormFile EmployeeFile { get; set; }  // For file upload (CSV)

        public List<Employee> Employees { get; set; }  // For manual employee data (if needed)
    }

}
