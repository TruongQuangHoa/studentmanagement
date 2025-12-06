using System.ComponentModel.DataAnnotations;

namespace StudentManagement.Models // Hoặc StudentManagement.ViewModels
{
    // Dữ liệu dùng để Đăng ký
    public class RegisterVM
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }
    }

    // Dữ liệu dùng để Đăng nhập
    public class LoginVM
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}