using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using StudentManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace StudentManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TeacherController : Controller
    {
        private readonly DataContext _context;
        public TeacherController(DataContext context)
        {
            _context = context;
        }
        public ActionResult Index()
        {
            var tcList = _context.Teachers.OrderBy(t => t.ID)
             .Include(t => t.department).ToList();
            return View(tcList);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var tc = _context.Teachers.Find(id);
            if (tc == null)
                return NotFound();
            return View(tc);
        }
        [HttpPost]

        public IActionResult Delete(int id)
        {
            var delTeacher = _context.Teachers.Find(id);
            if (delTeacher == null)
                return NotFound();
            _context.Teachers.Remove(delTeacher);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            getData();
            ViewBag.SelectedSubjects = new List<int>();
            return View();
        }

        [HttpPost]
        public IActionResult Create(tblTeacher model, int[] SelectedSubjectIDs)
        {
            // Kiểm tra xem TeacherID đã tồn tại chưa
            if (_context.Teachers.Any(t => t.TeacherID == model.TeacherID))
            {
                // Thêm lỗi vào ModelState
                ModelState.AddModelError("TeacherID", "Mã giáo viên này đã tồn tại trong hệ thống.");
            }

            if (ModelState.IsValid)
            {
                _context.Teachers.Add(model);
                _context.SaveChanges();

                // Thêm các môn học giáo viên dạy
                if (SelectedSubjectIDs != null && SelectedSubjectIDs.Length > 0)
                {
                    foreach (var subId in SelectedSubjectIDs)
                    {
                        _context.TeacherSubjects.Add(new tblTeacherSubject
                        {
                            TeacherID = model.TeacherID,
                            SubjectID = subId
                        });
                    }
                    _context.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            getData();
            ViewBag.SelectedSubjects = SelectedSubjectIDs?.ToList() ?? new List<int>();
            return View(model);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var teacher = _context.Teachers.Find(id);
            if (teacher == null) return NotFound();

            getData();

            // Lấy danh sách môn học giáo viên đang dạy 
            var selectedSubjects = _context.TeacherSubjects
                .Where(x => x.TeacherID == teacher.TeacherID)
                .Select(x => x.SubjectID ?? 0)
                .ToList();

            ViewBag.SelectedSubjects = selectedSubjects;

            return View(teacher);
        }
        // 
        [HttpPost]
        public IActionResult Edit(tblTeacher model, int[] SelectedSubjectIDs)
        {
            // Kiểm tra trùng lặp TeacherID, nhưng cho phép chính bản ghi đang chỉnh sửa
            var existingTeacher = _context.Teachers.AsNoTracking().FirstOrDefault(t => t.TeacherID == model.TeacherID);
            // Nếu bạn dùng ID là khóa chính, ta kiểm tra TeacherID trùng với giáo viên khác (ID khác)
            if (existingTeacher != null && existingTeacher.ID != model.ID)
            {
                ModelState.AddModelError("TeacherID", "Mã giáo viên này đã tồn tại cho một giáo viên khác.");
            }

            if (ModelState.IsValid)
            {
                _context.Teachers.Update(model);
                // Xóa môn học cũ
                var oldSubjects = _context.TeacherSubjects.Where(x => x.TeacherID == model.TeacherID);
                _context.TeacherSubjects.RemoveRange(oldSubjects);

                // Thêm môn học mới
                if (SelectedSubjectIDs != null && SelectedSubjectIDs.Length > 0)
                {
                    foreach (var subId in SelectedSubjectIDs)
                    {
                        _context.TeacherSubjects.Add(new tblTeacherSubject
                        {
                            TeacherID = model.TeacherID,
                            SubjectID = subId
                        });
                    }
                }
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            getData();
            ViewBag.SelectedSubjects = SelectedSubjectIDs?.ToList() ?? new List<int>();
            return View(model);
        }

        private void getData()
        {
            var dpList = _context.Departments.Where(d => d.IsActive == true)
                .Select(dp => new
                {
                    dp.DepartmentID,
                    Info = dp.DepartmentID + " - " + dp.DepartmentName
                }).ToList();
            ViewBag.dpList = new SelectList(dpList, "DepartmentID", "Info");

            var sbList = _context.Subjects
                .Where(sb => sb.IsActive)
                .Select(sb => new
                {
                    sb.SubjectID,
                    Info = sb.SubjectID + " - " + sb.SubjectName
                }).ToList();
            ViewBag.sbList = new SelectList(sbList, "SubjectID", "Info");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
                return NotFound();
            teacher.IsActive = !teacher.IsActive;
            _context.Update(teacher);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}