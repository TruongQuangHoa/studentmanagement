using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models; // Namespace chứa UserAccountViewModel
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace StudentManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới được vào đây
    public class UserAccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UserAccountController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        // 1. DANH SÁCH TÀI KHOẢN
        // GET: /Admin/UserAccount/Index
        public async Task<IActionResult> Index()
        {
            // Lấy tất cả user
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserAccountViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                // Lọc: Chỉ lấy tài khoản có quyền "Student" để hiển thị (bỏ if này nếu muốn hiện tất cả)
                if (roles.Contains("Student")) 
                {
                    userList.Add(new UserAccountViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        IsLocked = await _userManager.IsLockedOutAsync(user),
                        Roles = roles
                    });
                }
            }

            return View(userList);
        }

        // 2. CHỨC NĂNG KHÓA / MỞ KHÓA TÀI KHOẢN
        // GET: /Admin/UserAccount/ToggleLockStatus/username
        public async Task<IActionResult> ToggleLockStatus(string id, string returnUrl = null)
        {
            // Tìm theo UserName (hoặc Id tùy cách bạn truyền từ View, ở đây giả định id là UserName)
            var user = await _userManager.FindByNameAsync(id);
            if (user == null) return NotFound("Không tìm thấy tài khoản.");

            // Kiểm tra trạng thái khóa
            if (await _userManager.IsLockedOutAsync(user))
            {
                // Đang khóa -> Mở khóa (Set thời gian khóa về null)
                await _userManager.SetLockoutEndDateAsync(user, null); 
                TempData["Message"] = $"Đã MỞ KHÓA tài khoản {id}.";
            }
            else
            {
                // Đang mở -> Khóa vĩnh viễn (hoặc 100 năm)
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100)); 
                TempData["Message"] = $"Đã KHÓA tài khoản {id}.";
            }

            // Quay lại trang danh sách hoặc trang trước đó
            if (!string.IsNullOrEmpty(returnUrl)) return LocalRedirect(returnUrl);
            return RedirectToAction(nameof(Index)); 
        }

        // 3. CHỨC NĂNG RESET MẬT KHẨU (GET - Hiển thị form)
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id, string returnUrl = null)
        {
            var user = await _userManager.FindByNameAsync(id);
            if (user == null) return NotFound("Không tìm thấy tài khoản.");

            // Lưu returnUrl vào ViewBag để nút "Quay lại" biết đường về
            ViewBag.ReturnUrl = returnUrl;
            
            // Truyền UserName sang View làm model
            return View(model: id);
        }

        // 4. CHỨC NĂNG RESET MẬT KHẨU (POST - Xử lý logic)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string newPassword, string returnUrl = null)
        {
            var user = await _userManager.FindByNameAsync(id);
            if (user == null) return NotFound();

            // Bắt buộc phải tạo token reset mật khẩu
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Thực hiện đổi mật khẩu
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                TempData["Message"] = $"Đổi mật khẩu cho tài khoản {id} thành công!";
                
                if (!string.IsNullOrEmpty(returnUrl)) return LocalRedirect(returnUrl);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Nếu lỗi (ví dụ mật khẩu quá yếu), hiển thị lỗi ra View
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                
                ViewBag.ReturnUrl = returnUrl;
                return View(model: id);
            }
        }
    }
}