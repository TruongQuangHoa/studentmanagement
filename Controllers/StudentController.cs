using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;
using System.Linq;

namespace StudentManagement.Controllers
{
    public class StudentController : Controller
    {
        private readonly DataContext _context;

        public StudentController(DataContext context)
        {
            _context = context;
        }

        // GET: /Student/Profile?id=123
        public IActionResult Profile(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID học sinh không hợp lệ.");
            }

            // Lấy sinh viên theo StudentID
            var student = _context.Students.FirstOrDefault(s => s.StudentID == id);

            if (student == null)
            {
                return NotFound("Không tìm thấy học sinh.");
            }

            // Trả về view Profile.cshtml trong Views/Student/
            return View("Profile", student);
;
        }
    }
}
