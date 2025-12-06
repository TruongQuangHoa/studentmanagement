using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudentManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace StudentManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StudentController : Controller
    {
        private readonly DataContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public StudentController(DataContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // --- 1. INDEX (Đã thêm logic Tìm kiếm và Lọc lớp) ---
        public IActionResult Index(int? classId, string query)
        {
            // Eager load dữ liệu lớp
            var students = _context.Students
                .Include(s => s.studentclass!)
                    .ThenInclude(st => st._class)
                .AsQueryable();

            // Lọc theo Lớp
            if (classId.HasValue && classId > 0)
            {
                students = students.Where(s => s.studentclass!.Any(st => st.ClassID == classId && st.IsActive));
                ViewBag.CurrentClassId = classId;
            }

            // Lọc theo Từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(query))
            {
                students = students.Where(s => s.FullName.Contains(query) || s.StudentID.Contains(query));
                ViewBag.CurrentQuery = query;
            }

            // Load dropdown lớp học cho Form lọc
            LoadData();
            
            return View(students.OrderByDescending(h => h.ID).ToList());
        }

        // --- 2. CREATE (GET) - HIỂN THỊ FORM (QUAN TRỌNG: SỬA LỖI 405) ---
        [HttpGet]
        public IActionResult Create()
        {
            LoadData(); // Load danh sách lớp để chọn
            return View();
        }

        // --- 3. CREATE (POST) - XỬ LÝ LƯU ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(tblStudent student, int? ClassID) // Thêm ClassID để lưu lớp luôn
        {
            // Kiểm tra xem StudentID đã tồn tại chưa
            if (_context.Students.Any(t => t.StudentID == st.StudentID))
            {
                // Thêm lỗi vào ModelState
                ModelState.AddModelError("StudentID", "Mã học sinh này đã tồn tại trong hệ thống.");
            }

            if (ModelState.IsValid)
            {
                // A. TẠO TÀI KHOẢN IDENTITY
                var user = new IdentityUser
                {
                    UserName = student.StudentID,
                    Email = (student.StudentID) + "@truongthpt.edu.vn"
                };

                var result = await _userManager.CreateAsync(user, "Hocsinh@123");

                if (result.Succeeded)
                {
                    // Kiểm tra xem Role "Student" đã có trong DB chưa
                    if (!await _roleManager.RoleExistsAsync("Student"))
                    {
                        // Nếu chưa có thì tạo mới ngay lập tức
                        await _roleManager.CreateAsync(new IdentityRole("Student"));
                    }
                    
                    await _userManager.AddToRoleAsync(user, "Student");

                    // B. LƯU HỒ SƠ HỌC SINH
                    student.IsActive = true;
                    _context.Add(student);
                    await _context.SaveChangesAsync();

                    // C. LƯU PHÂN LỚP (Nếu có chọn lớp)
                    if (ClassID.HasValue && ClassID > 0)
                    {
                        var relation = new tblStudentClass
                        {
                            StudentID = student.StudentID,
                            ClassID = ClassID.Value,
                            IsActive = true
                        };
                        _context.StudentClasses.Add(relation);
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", "Lỗi tạo tài khoản: " + error.Description);
                    }
                }
            }
            
            LoadData(); // Load lại dropdown nếu lỗi
            return View(student);
        }

        // --- 4. EDIT (GET) ---
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var student = _context.Students
                 .Include(s => s.studentclass)
                 .FirstOrDefault(s => s.ID == id);

            if (student == null) return NotFound();

            // Tìm lớp hiện tại đang học (IsActive = true)
            var currentClass = student.studentclass?.FirstOrDefault(st => st.IsActive);
            if (currentClass != null)
            {
                ViewBag.CurrentClassID = currentClass.ClassID;
            }

            LoadData();
            return View(student);
        }

        // --- 5. EDIT (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(tblStudent st, int? ClassID)
        {   
            // Kiểm tra trùng lặp StudentID, nhưng cho phép chính bản ghi đang chỉnh sửa
            var existingStudent = _context.Students.AsNoTracking().FirstOrDefault(t => t.StudentID == st.StudentID);
            
            // Nếu bạn dùng ID là khóa chính, ta kiểm tra StudentID trùng với học sinh khác (ID khác)
            if (existingStudent != null && existingStudent.ID != st.ID)
            {
                ModelState.AddModelError("StudentID", "Mã học sinh này đã tồn tại cho một học sinh khác.");
            }

            if (ModelState.IsValid)
            {
                _context.Update(st);

                // Xử lý cập nhật lớp học
                var currentRelation = _context.StudentClasses
                    .FirstOrDefault(sc => sc.StudentID == st.StudentID && sc.IsActive);

                if (ClassID.HasValue && ClassID > 0)
                {
                    // Nếu lớp chọn MỚI khác lớp CŨ
                    if (currentRelation == null || currentRelation.ClassID != ClassID)
                    {
                        // 1. Hủy lớp cũ (nếu có)
                        if (currentRelation != null)
                        {
                            currentRelation.IsActive = false;
                            _context.Update(currentRelation);
                        }

                        // 2. Thêm lớp mới
                        var newRelation = new tblStudentClass
                        {
                            StudentID = st.StudentID,
                            ClassID = ClassID.Value,
                            IsActive = true
                        };
                        _context.StudentClasses.Add(newRelation);
                    }
                }
                else if (currentRelation != null)
                {
                    // Nếu người dùng bỏ chọn lớp -> Hủy lớp hiện tại
                    currentRelation.IsActive = false;
                    _context.Update(currentRelation);
                }

                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            LoadData();
            return View(st);
        }

        // --- 6. DELETE ---
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();
            var st = _context.Students.Find(id);
            if (st == null) return NotFound();
            return View(st);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var delStudent = _context.Students.Find(id);
            if (delStudent == null) return NotFound();

            // Xóa quan hệ lớp học trước
            var classRelations = _context.StudentClasses.Where(st => st.StudentID == delStudent.StudentID);
            _context.StudentClasses.RemoveRange(classRelations);

            // Xóa học sinh
            _context.Students.Remove(delStudent);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // --- 7. TOGGLE STATUS ---
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();
            
            student.IsActive = !student.IsActive;
            _context.Update(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // --- HELPER: LOAD DATA CHO DROPDOWN ---
        public void LoadData()
        {
            var clList = _context.Classes
                .Include(cl => cl.grade)
                .Where(cl => cl.IsActive == true)
                .Select(cl => new
                {
                    cl.ClassID,
                    // Sửa tên property cho khớp với SelectList bên dưới
                    ThongTin = cl.ClassName + (cl.grade != null ? " | Khối: " + cl.grade.GradeName : "")
                })
                .ToList();

            // Sửa tên ViewBag thành LopHocList để khớp với View Index.cshtml
            // Sửa tham số thứ 3 thành "ThongTin" (khớp với Select ở trên)
            ViewBag.LopHocList = new SelectList(clList, "ClassID", "ThongTin");
        }
    }
}