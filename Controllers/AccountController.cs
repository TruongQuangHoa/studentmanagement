using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace StudentManagement.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Thay vì chuyển hướng tự động, ta kiểm tra:
            // Nếu đã đăng nhập -> Trả về View nhưng kèm thông báo (hoặc dùng View riêng)
            if (User?.Identity?.IsAuthenticated == true)
            {
                // Cách an toàn nhất: Vẫn hiện trang Login nhưng báo là đã đăng nhập
                // Hoặc đơn giản là cứ hiện trang Login để họ có thể đăng nhập nick khác
                // => GIỮ NGUYÊN return View() là an toàn nhất để tránh lặp!
                return View();
            }

            return View();
        }

        public IActionResult Logout()
        {
            // Xóa cookie phía server
            Response.Cookies.Delete("X-Access-Token");
            return RedirectToAction("Login");
        }
    }
}