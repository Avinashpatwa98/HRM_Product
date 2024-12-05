using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Attendance_maintaince.Models
{
    public class Employee
    {
        public Guid? EmployeeId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Position { get; set; } // Make it nullable

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } // Admin or Employee
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow; // Default to current date and time

        [NotMapped]
        public List<SelectListItem> PositionOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Software Backend Engineer", Text = "Software Backend Engineer" },
            new SelectListItem { Value = "Frontend", Text = "Frontend" },
            new SelectListItem { Value = "Full Stack", Text = "Full Stack" },
            new SelectListItem { Value = "Market Management", Text = "Market Management" },
            new SelectListItem { Value = "Project Manager", Text = "Project Manager" },
            new SelectListItem { Value = "IT Cell", Text = "IT Cell" },
            new SelectListItem { Value = "HR", Text = "HR" },
            new SelectListItem { Value = "Custom", Text = "Custom" }  // Custom option
        };
        [NotMapped]
        public string? SelectedPosition { get; set; }
        [NotMapped]
        public string? CustomPosition { get; set; }
    }
}
