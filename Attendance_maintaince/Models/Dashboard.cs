namespace Attendance_maintaince.Models
{
    public class Dashboard
    {
        public string UserName { get; set; } // User's name
        public string UserProfilePictureUrl { get; set; } // User's profile picture URL
        public List<Holiday> Holidays { get; set; }
        public List<User> OnLeaveToday { get; set; }
        public List<User> WorkingRemotely { get; set; }
        public List<Announcement> Announcements { get; set; }
    }

    public class Holiday
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }

    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ProfilePictureUrl { get; set; }
    }

    public class Announcement
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
    }
}


