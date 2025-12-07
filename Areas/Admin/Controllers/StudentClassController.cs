using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;
//using OfficeOpenXml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;


namespace StudentManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Bắt buộc: Chỉ Admin mới được vào
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
                        .Include(h => h.yearSemester)
                        .Include(h => h._class)
                            .ThenInclude(l => l!.grade)
                        .ToList();
            // LoadStudentDataForCreate();
            LoadData();
            return View(scList);
        }

        // Cập nhật số học sinh hiện tại
        private void UpdateCurrentStudents(int classId)
        {
            var _class = _context.Classes.Find(classId);
            if (_class != null)
            {
                // Đếm số học sinh duy nhất trong lớp (Distinct theo StudentID)
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

        //[HttpPost]
        // private void LoadStudentDataForCreate()
        // {
        //     // Lấy danh sách StudentID của tất cả học sinh đang hoạt động trong bất kỳ lớp nào
        //     var existingStudentIds = _context.StudentClasses
        //         .Where(sc => sc.IsActive == true)
        //         .Select(sc => sc.StudentID)
        //         .Distinct()
        //         .ToList();

        //     // Lọc danh sách học sinh: chỉ lấy những học sinh đang hoạt động VÀ chưa có trong lớp học
        //     var stList = _context.Students
        //         .Where(c => c.IsActive == true && !existingStudentIds.Contains(c.StudentID))
        //         .Select(st => new
        //         {
        //             st.StudentID,
        //             Info = st.StudentID + " - " + st.FullName
        //         }).ToList();

        //     // Tạo SelectList cho ViewBag.StudentList
        //     ViewBag.StudentList = new SelectList(stList, "StudentID", "Info");
        // }

        private void LoadData(int? selectedClassID = null, int? selectedSemesterID = null, int? selectedCohortID = null)
        {
            // Danh sách học sinh
            var students = _context.Students
               .Select(s => new { Value = s.StudentID, Text = s.FullName })
               .ToList();

            var stList = _context.Students.Where(c => c.IsActive == true)
                        .Select(hs => new
                        {
                            hs.StudentID,
                            Info = hs.StudentID + " - " + hs.FullName
                        }).ToList();
            ViewBag.StudentList = new SelectList(stList, "StudentID", "Info");

            // Danh sách lớp
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

            var semesterList = _context.YearSemesters
                .Where(s => s.IsActive)
                .Select(s => new { s.YearSemesterID, Text = s.SemesterName + " | Năm học: " + s.SchoolYear })
                .ToList();

            ViewBag.SemesterList = new SelectList(semesterList, "YearSemesterID", "Text", selectedSemesterID); // <== Cần thêm vào

            var chList = _context.Cohorts
                .Where(c => c.IsActive)
                .Select(c => new { c.CohortID, Text = (c.StartYear + "-" + c.EndYear) + " | K" + c.CohortName })
                .ToList();

            ViewBag.CohortList = new SelectList(chList, "CohortID", "Text", selectedCohortID);
        }

        public IActionResult Create()
        {
            //LoadStudentDataForCreate(); // Load StudentList cho trang Create
            LoadData(); // Load ClassList và CohortList và SemesterList
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(tblStudentClass model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ClassID"] = new SelectList(_context.Classes, "ClassID", "ClassName", model.ClassID);
                ViewData["StudentID"] = new SelectList(_context.Students, "StudentID", "FullName", model.StudentID);
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

            // Lấy tất cả học kỳ thuộc niên khóa của lớp
            var semesters = _context.YearSemesters
                .Where(s => s.SchoolYear == _class.SchoolYear && s.IsActive)
                .ToList();

            if (!semesters.Any())
            {
                TempData["Error"] = $"Chưa khai báo học kỳ cho niên khóa {_class.SchoolYear}.";
                return RedirectToAction(nameof(Index));
            }

            int addedCount = 0;
            foreach (var semester in semesters)
            {
                // Kiểm tra trùng
                bool exists = _context.StudentClasses
                    .Any(h => h.StudentID == model.StudentID &&
                              h.ClassID == model.ClassID &&
                              h.YearSemesterID == semester.YearSemesterID &&
                              h.CohortID == _class.CohortID &&
                              h.IsActive);

                if (exists) continue;

                var newEntry = new tblStudentClass
                {
                    StudentID = model.StudentID,
                    ClassID = model.ClassID,
                    YearSemesterID = semester.YearSemesterID,
                    CohortID = _class.CohortID,
                    IsActive = true
                };

                _context.StudentClasses.Add(newEntry);
                addedCount++;
            }

            await _context.SaveChangesAsync();
            UpdateCurrentStudents(model.ClassID);

            TempData["Success"] = $"Đã thêm {addedCount} bản ghi học sinh vào lớp {_class.ClassName} (theo đầy đủ học kỳ/niên khóa).";
            return RedirectToAction(nameof(Index));
        }

        // public IActionResult CreateFromExcel()
        // {
        //     LoadDuLieu(); // Nạp ViewBag.ClassList
        //     return View();
        // }

        // [HttpPost]
        // public async Task<IActionResult> CreateFromExcel(IFormFile UploadedFile, int classId)
        // {
        //     if (UploadedFile == null || UploadedFile.Length == 0)
        //     {
        //         TempData["Error"] = "Chưa chọn file Excel.";
        //         return RedirectToAction("Index");
        //     }

        //     var lopHoc = _context.QLLopHocs
        //         .Include(l => l.Khois)
        //         .Include(l => l.KhoaHoc)
        //         .FirstOrDefault(l => l.ClassID == classId && l.IsActive);

        //     if (lopHoc == null)
        //     {
        //         TempData["Error"] = "Lớp học không tồn tại hoặc không còn hoạt động.";
        //         return RedirectToAction("Index");
        //     }

        //     // Lấy danh sách học kỳ thuộc niên khóa của lớp
        //     var semesters = _context.QLHocKys
        //         .Where(s => s.semester_code == lopHoc.SchoolYear && s.IsActive)
        //         .ToList();

        //     if (!semesters.Any())
        //     {
        //         TempData["Error"] = $"Chưa khai báo học kỳ cho niên khóa {lopHoc.SchoolYear}.";
        //         return RedirectToAction("Index");
        //     }

        //     int addedCount = 0;
        //     using var stream = new MemoryStream();
        //     await UploadedFile.CopyToAsync(stream);

        //     using var package = new ExcelPackage(stream);
        //     var worksheet = package.Workbook.Worksheets[0];
        //     int rowCount = worksheet.Dimension.Rows;

        //     for (int row = 2; row <= rowCount; row++) // bỏ dòng header
        //     {
        //         string studentId = worksheet.Cells[row, 2].Text?.Trim();
        //         if (string.IsNullOrEmpty(studentId)) continue;

        //         studentId = new string(studentId.Where(c => !char.IsControl(c)).ToArray());

        //         var hs = _context.QLHocSinhs.FirstOrDefault(s => s.StudentID == studentId);
        //         if (hs == null) continue;

        //         // Kiểm tra lớp đầy
        //         int currentCount = _context.QLHocSinhLopHocs.Count(h => h.ClassID == classId && h.IsActive);
        //         if (currentCount >= lopHoc.MaxStudents) break;

        //         foreach (var semester in semesters)
        //         {
        //             bool exists = _context.QLHocSinhLopHocs
        //                 .Any(h => h.StudentID == studentId &&
        //                           h.ClassID == classId &&
        //                           h.SemesterID == semester.SemesterID &&
        //                           h.CourseID == lopHoc.CourseID &&
        //                           h.IsActive);

        //             if (exists) continue;

        //             _context.QLHocSinhLopHocs.Add(new QLHocSinhLopHoc
        //             {
        //                 StudentID = studentId,
        //                 ClassID = classId,
        //                 SemesterID = semester.SemesterID,
        //                 CourseID = lopHoc.CourseID,
        //                 IsActive = true
        //             });

        //             addedCount++;
        //         }
        //     }

        //     await _context.SaveChangesAsync();
        //     UpdateCurrentStudents(classId);

        //     TempData["Success"] = $"Đã thêm {addedCount} bản ghi học sinh (theo đầy đủ học kỳ/niên khóa) vào lớp {lopHoc.ClassName}.";
        //     return RedirectToAction("Index");
        // }

        public IActionResult Edit(int id)
        {
            var entity = _context.StudentClasses
                .Include(h => h.student)
                .Include(h => h._class)
                .FirstOrDefault(h => h.StudentClassID == id);

            if (entity == null) return NotFound();

            LoadData(entity.ClassID, entity.YearSemesterID, entity._class?.CohortID);
            return View(entity);
        }

        [HttpPost]
        public IActionResult Edit(tblStudentClass model)
        {
            if (!ModelState.IsValid)
            {
                LoadData(model.ClassID, model.YearSemesterID);
                return View(model);
            }

            var existing = _context.StudentClasses
                .FirstOrDefault(h => h.StudentClassID == model.StudentClassID);

            if (existing == null)
            {
                ModelState.AddModelError("", "Không tìm thấy dữ liệu để cập nhật.");
                LoadData(model.ClassID, model.YearSemesterID);
                return View(model);
            }

            var oldClassID = existing.ClassID; // Lưu lại lớp cũ trước khi thay đổi
            bool isChangingClass = oldClassID != model.ClassID;

            // === KIỂM TRA TRÙNG LẶP (giữ nguyên logic bạn đã viết tốt) ===
            bool exists = _context.StudentClasses
                .Any(h => h.StudentID == model.StudentID &&
                          h.YearSemesterID == model.YearSemesterID &&
                          h.ClassID == model.ClassID &&
                          h.StudentClassID != model.StudentClassID &&
                          h.IsActive);

            if (exists)
            {
                ModelState.AddModelError("", "Học sinh đã được đăng ký vào lớp này trong học kỳ này.");
                LoadData(model.ClassID, model.YearSemesterID);
                return View(model);
            }

            bool existsInOtherClassInSameSemester = _context.StudentClasses
                .Any(h => h.StudentID == model.StudentID &&
                          h.YearSemesterID == model.YearSemesterID &&
                          h.ClassID != model.ClassID &&
                          h.StudentClassID != model.StudentClassID &&
                          h.IsActive);

            if (existsInOtherClassInSameSemester)
            {
                ModelState.AddModelError("", "Học sinh đã đăng ký ở lớp khác trong cùng học kỳ này.");
                LoadData(model.ClassID, model.YearSemesterID);
                return View(model);
            }

            try
            {
                // Cập nhật dữ liệu
                existing.StudentID = model.StudentID;
                existing.ClassID = model.ClassID;
                existing.YearSemesterID = model.YearSemesterID;
                existing.IsActive = model.IsActive;

                _context.Update(existing);
                _context.SaveChanges();

                // === CẬP NHẬT LẠI SỐ LƯỢNG HỌC SINH CHO CẢ LỚP CŨ VÀ LỚP MỚI ===
                if (isChangingClass)
                {
                    // Cập nhật lớp cũ (giảm 1)
                    UpdateCurrentStudents(oldClassID);

                    // Cập nhật lớp mới (tăng 1)
                    UpdateCurrentStudents(model.ClassID);
                }
                else
                {
                    // Nếu chỉ thay đổi trạng thái IsActive hoặc học kỳ → vẫn cần cập nhật lại chính xác
                    UpdateCurrentStudents(model.ClassID);
                }

                TempData["Success"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật: " + ex.Message);
                LoadData(model.ClassID, model.YearSemesterID);
                return View(model);
            }
        }

        public IActionResult Delete(int id)
        {
            var entity = _context.StudentClasses.Include(h => h.student)
                                                  .Include(h => h._class)
                                                  .ThenInclude(l => l!.grade)
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

        // public IActionResult ExportExcel(int? classId)
        // {
        //     // Lấy danh sách học sinh – lớp
        //     var list = _context.QLHocSinhLopHocs
        //                 .Include(h => h.hocsinh)
        //                 .Include(h => h.lopHoc)
        //                 .AsQueryable();

        //     if (classId.HasValue && classId.Value > 0)
        //         list = list.Where(h => h.ClassID == classId.Value);

        //     // Chỉ lấy duy nhất mỗi học sinh trong một lớp
        //     var data = list
        //         .GroupBy(h => new { h.StudentID, h.ClassID })
        //         .Select(g => g.First())
        //         .OrderBy(h => h.lopHoc.ClassName)
        //         .ThenBy(h => h.hocsinh.FullName)
        //         .ToList();

        //     using (var package = new ExcelPackage())
        //     {
        //         var ws = package.Workbook.Worksheets.Add("HocSinhLopHoc");

        //         // Header
        //         ws.Cells[1, 1].Value = "STT";
        //         ws.Cells[1, 2].Value = "StudentID";
        //         ws.Cells[1, 3].Value = "Họ tên";
        //         ws.Cells[1, 4].Value = "Tên lớp";
        //         ws.Cells[1, 5].Value = "Niên khóa";

        //         int row = 2;
        //         foreach (var item in data)
        //         {
        //             ws.Cells[row, 1].Value = row - 1;
        //             ws.Cells[row, 2].Value = item.StudentID;
        //             ws.Cells[row, 3].Value = item.hocsinh?.FullName ?? "";
        //             ws.Cells[row, 4].Value = item.lopHoc?.ClassName ?? "";
        //             ws.Cells[row, 5].Value = item.lopHoc?.SchoolYear ?? "";
        //             row++;
        //         }

        //         ws.Cells[ws.Dimension.Address].AutoFitColumns();

        //         var stream = new MemoryStream();
        //         package.SaveAs(stream);
        //         stream.Position = 0;

        //         string excelName = $"HocSinhLopHoc-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        //         return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        //     }
        // }
    }
}
