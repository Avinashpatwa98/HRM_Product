namespace Attendance_maintaince.Models
{
  
    public class Attendance
    {
        public int ID { get; set; }            // Primary key
        public Guid EmployeeID { get; set; }    // Employee ID
        public string? EmployeeName { get; set; } // Employee Name
        public DateTime ComingTime { get; set; } // Time when the employee arrives
        public string? DateOfDay { get; set; } // Date of the attendance
        public bool IsPresent { get; set; }    // Status (true if present, false if absent)
        public DateTime? LeaveTime { get; set; } // Time when the employee leaves, nullable
    }

}
