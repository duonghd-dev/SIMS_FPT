using System.Collections.Generic;
using SIMS_FPT.Models;
using System.ComponentModel.DataAnnotations;
namespace SIMS_FPT.Models.ViewModels
{
    public class StudentProfileViewModel
    {
        public StudentCSVModel Student { get; set; } = new StudentCSVModel();
        public List<AssignmentHistoryItem> AssignmentHistory { get; set; } = new();
        public double AverageScorePercent { get; set; }
        // Thông tin hiển thị (Read-only hoặc Hidden)
        public string StudentId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Thông tin cá nhân có thể sửa
        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DOB { get; set; }

        public string? Gender { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string? AvatarPath { get; set; }

        // --- Phần Đổi Mật Khẩu (Optional) ---
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string? ConfirmNewPassword { get; set; }
    }

    public class AssignmentHistoryItem
    {
        public string AssignmentId { get; set; } = string.Empty;
        public string AssignmentTitle { get; set; } = string.Empty;
        public string SubjectId { get; set; } = string.Empty;
        public double? Grade { get; set; }
        public int MaxPoints { get; set; }
        public string TeacherComments { get; set; } = string.Empty;
        public bool Submitted => Grade.HasValue || !string.IsNullOrEmpty(TeacherComments);
    }
}


