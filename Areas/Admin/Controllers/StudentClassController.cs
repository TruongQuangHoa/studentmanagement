using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
using Microsoft.AspNetCore.Http;


namespace StudentManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StudentClassController : Controller
    {
        private readonly DataContext _context;

        public StudentClassController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var scList = _context.StudentClasses
                        .Include(h => h.student)
                        .Include(h => h._class)
                            .ThenInclude(l => l.grade)
                        .ToList();
            LoadData();
            return View(scList);
        }

        private void UpdateCurrentStudents(int classId)
        {
            var _class = _context.Classes.Find(classId);
            if (_class != null)
            {
                int uniqueStudentCount = _context.StudentClasses
                    .Where(h => h.ClassID == classId && h.IsActive)
                    .Select(h => h.StudentID)
                    .Distinct()
                    .Count();

                _class.CurrentStudents = uniqueStudentCount;
                _context.Update(_class);
                _context.SaveChanges();
            }
        }

        private void LoadData(int? selectedClassID = null, int? selectedSemesterID = null, int? selectedCourseID = null)
        {
            var students = _context.Students
                .Select(s => new { Value = s.StudentID, Text = s.FullName })
                .ToList();

            var stList = _context.Students.Where(c => c.IsActive == true)
                        .Select(st => new
                        {
                            st.StudentID,
                            Info = st.StudentID + " - " + st.FullName
                        }).ToList();
            ViewBag.StudentList = new SelectList(stList, "StudentID", "Info");

            var clList = _context.Classes
                .Include(l => l.grade)
                .Include(l => l.cohort)
                .Where(l => l.IsActive)
                .Select(l => new
                {
                    l.ClassID,
                    Info = l.ClassName
                               + " | Khối: " + (l.grade != null ? l.grade.GradeName : "N/A")
                               + " | Khóa: " + (l.cohort != null ? (l.cohort.StartYear + "-" + l.cohort.EndYear) : "N/A")
                               + " | Chỗ còn: " + (l.MaxStudents - l.CurrentStudents)
                })
                .OrderBy(l => l.Info)
                .ToList();

            ViewBag.ClassList = new SelectList(clList, "ClassID", "Info", selectedClassID);

            var chList = _context.Cohorts
                .Where(c => c.IsActive)
                .Select(c => new { c.CohortID, Text = c.CohortName })
                .ToList();

            ViewBag.CohortList = new SelectList(chList, "CohortID", "Text", selectedCourseID);
        }

        public IActionResult Create()
        {
            LoadData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(tblStudentClass model)
        {
            if (!ModelState.IsValid)
            {
                LoadData(model.ClassID);
                return View(model);
            }

            var _class = _context.Classes
                .Include(l => l.cohort)
                .FirstOrDefault(l => l.ClassID == model.ClassID && l.IsActive);

            if (_class == null)
            {
                TempData["Error"] = "Lớp học không tồn tại hoặc không hoạt động.";
                return RedirectToAction(nameof(Index));
            }

            // 🔎 Kiểm tra học sinh đã có trong lớp chưa
            bool exists = _context.StudentClasses
                .Any(s => s.StudentID == model.StudentID && s.ClassID == model.ClassID && s.IsActive);

            if (exists)
            {
                TempData["Error"] = "Học sinh này đã có trong lớp.";
                LoadData(model.ClassID);
                return View(model);
            }

            // ✅ Thêm mới bản ghi
            model.IsActive = true;
            _context.StudentClasses.Add(model);
            await _context.SaveChangesAsync();

            // ✅ Cập nhật lại số lượng học sinh hiện tại của lớp
            UpdateCurrentStudents(model.ClassID);

            TempData["Success"] = "Đã thêm học sinh vào lớp thành công!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var entity = _context.StudentClasses
                .Include(h => h.student)
                .Include(h => h._class)
                .FirstOrDefault(h => h.StudentClassID == id);

            if (entity == null) return NotFound();

            // Truyền các ID hiện tại để preselect dropdown
            LoadData(entity.ClassID, null, entity._class?.CohortID);
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(tblStudentClass model)
        {
            if (!ModelState.IsValid)
            {
                LoadData(model.ClassID);
                return View(model);
            }

            var existing = _context.StudentClasses
                .FirstOrDefault(h => h.StudentClassID == model.StudentClassID);

            if (existing == null)
            {
                ModelState.AddModelError("", "Không tìm thấy dữ liệu để cập nhật.");
                LoadData(model.ClassID);
                return View(model);
            }

            var _class = _context.Classes
                .Include(l => l.grade)
                .Include(l => l.cohort)
                .FirstOrDefault(l => l.ClassID == model.ClassID && l.IsActive);

            if (_class == null)
            {
                ModelState.AddModelError("ClassID", "Lớp học không tồn tại hoặc không còn hoạt động.");
                LoadData(model.ClassID);
                return View(model);
            }

            // Kiểm tra trùng lặp
            bool exists = _context.StudentClasses
                .Include(h => h._class)
                .Any(h => h.StudentID == model.StudentID &&
                          h._class.SchoolYear == _class.SchoolYear &&
                          h._class.GradeID == _class.GradeID &&
                          h._class.CohortID == _class.CohortID &&
                          h.StudentClassID != model.StudentClassID &&
                          h.IsActive);

            if (exists)
            {
                ModelState.AddModelError("StudentID", "Học sinh đã đăng ký lớp cùng khối, khóa và năm học.");
                LoadData(model.ClassID);
                return View(model);
            }

            try
            {
                // ✅ Chỉ cập nhật các thuộc tính cho phép
                existing.StudentID = model.StudentID;
                existing.ClassID = model.ClassID;
                existing.IsActive = model.IsActive;

                _context.SaveChanges();

                // ✅ Cập nhật lại số lượng học sinh
                UpdateCurrentStudents(existing.ClassID);
                if (existing.ClassID != model.ClassID)
                    UpdateCurrentStudents(model.ClassID);

                TempData["Success"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật: " + ex.Message);
                LoadData(model.ClassID);
                return View(model);
            }
        }


        public IActionResult Delete(int id)
        {
            var entity = _context.StudentClasses.Include(h => h.student)
                                                  .Include(h => h._class)
                                                  .FirstOrDefault(h => h.StudentClassID == id);
            if (entity == null) return NotFound();
            return View(entity);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var entity = _context.StudentClasses.Find(id);
            if (entity != null)
            {
                int classId = entity.ClassID;
                _context.StudentClasses.Remove(entity);
                _context.SaveChanges();
                UpdateCurrentStudents(classId);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var entity = _context.StudentClasses.Find(id);
            if (entity != null)
            {
                entity.IsActive = !entity.IsActive;
                _context.Update(entity);
                _context.SaveChanges();
                UpdateCurrentStudents(entity.ClassID);
            }

            return RedirectToAction("Index");
        }
    }
}
