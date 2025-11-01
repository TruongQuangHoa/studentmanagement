using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StudentManagement.Models;

namespace StudentManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StudentController : Controller
    {
        private readonly DataContext _context;

        public StudentController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? classID = null)
        {
            // ViewBag.IsAdmin = Functions.IsAdmin(HttpContext);
            // ViewBag.IsTeacher = Functions.IsTeacher(HttpContext);

            var stList = _context.Students
                .Include(s => s.studentclass)
                    .ThenInclude(st => st._class)
                        .ThenInclude(l => l.grade)
                .AsQueryable();

            if (classID.HasValue)
            {
                stList = stList.Where(s => s.studentclass.Any(st => st.ClassID == classID.Value && st.IsActive));
                ViewBag.SelectedClass = classID.Value.ToString();
            }
            else
            {
                ViewBag.SelectedClass = "";
            }

            return View(stList.OrderBy(h => h.ID).ToList());
        }

        // public IActionResult ExportToExcel(int? classId)
        // {
        //     try
        //     {
        //         var query = _context.QLHocSinhs
        //             .Include(h => h.HocSinhLopHocs)
        //                 .ThenInclude(hsl => hsl.lopHoc)
        //             .Where(h => h.HocSinhLopHocs.Any(hsl => hsl.IsActive))
        //             .AsQueryable();

        //         string fileLabel = "toantruong";

        //         if (classId.HasValue && classId.Value > 0)
        //         {
        //             query = query.Where(h => h.HocSinhLopHocs.Any(hsl => hsl.ClassID == classId.Value && hsl.IsActive));
        //             var className = _context.QLLopHocs
        //                 .Where(l => l.ClassID == classId)
        //                 .Select(l => l.ClassName)
        //                 .FirstOrDefault();
        //             fileLabel = className?.Replace(" ", "_") ?? $"Lop_{classId}";
        //         }

        //         var rawData = query.ToList();

        //         if (!rawData.Any())
        //             return Content("Không có học sinh nào để xuất Excel.");

        //         var data = rawData.Select((h, index) => new
        //         {
        //             STT = index + 1,
        //             StudentID = h.StudentID,
        //             FullName = h.FullName,
        //             Birth = h.Birth?.ToString("dd/MM/yyyy") ?? "Không có dữ liệu",
        //             Gender = h.Gender,
        //             Address = h.Address,
        //             ClassName = h.HocSinhLopHocs.FirstOrDefault(hsl => hsl.IsActive)?.lopHoc?.ClassName ?? "Chưa có lớp",
        //             Nation = h.Nation,
        //             Religion = h.Religion,
        //             NumberPhone = h.NumberPhone
        //         }).ToList();

        //         var stream = new MemoryStream();
        //         using (var package = new ExcelPackage(stream))
        //         {
        //             var sheet = package.Workbook.Worksheets.Add($"DanhSach_{fileLabel}");
        //             sheet.Cells.LoadFromCollection(data, true);
        //             sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        //             package.Save();
        //         }

        //         stream.Position = 0;
        //         var fileName = $"DanhSachHocSinh_{fileLabel}.xlsx";
        //         return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        //     }
        //     catch (Exception ex)
        //     {
        //         System.Diagnostics.Debug.WriteLine($"Lỗi khi xuất Excel: {ex.Message}");
        //         return Content("Đã xảy ra lỗi khi xuất file Excel.");
        //     }
        // }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var st = _context.Students.Find(id);
            if (st == null)
                return NotFound();
            return View(st);
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var delStudent = _context.Students.Find(id);
            if (delStudent == null)
                return NotFound();

            var classRelations = _context.StudentClasses.Where(st => st.StudentID == delStudent.StudentID);
            _context.StudentClasses.RemoveRange(classRelations);

            _context.Students.Remove(delStudent);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(tblStudent st, int? ClassID)
        {
            if (ModelState.IsValid)
            {
                _context.Students.Add(st);
                if (ClassID.HasValue && ClassID > 0)
                {
                    var sc = new tblStudentClass
                    {
                        StudentID = st.StudentID,
                        ClassID = ClassID.Value,
                        IsActive = true
                    };
                    _context.StudentClasses.Add(sc);
                }

                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(st);
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

           var student = _context.Students
                .Include(s => s.studentclass)
                .FirstOrDefault(s => s.ID == id);

            if (student == null)
                return NotFound();

            var currentClass = student.studentclass.FirstOrDefault(st => st.IsActive);
            if (currentClass != null)
            {
                ViewBag.CurrentClassID = currentClass.ClassID;
            }

            return View(student);
        }
        [HttpPost]
        public IActionResult Edit(tblStudent st, int? ClassID)
        {
            if (ModelState.IsValid)
            {
                _context.Update(st);

                var currentRelation = _context.StudentClasses
                    .FirstOrDefault(sc => sc.StudentID == st.StudentID && sc.IsActive);

                if (ClassID.HasValue && ClassID > 0)
                {
                    if (currentRelation != null)
                    {
                        currentRelation.IsActive = false;
                        _context.Update(currentRelation);
                    }

                    var newRelation = new tblStudentClass
                    {
                        StudentID = st.StudentID,
                        ClassID = ClassID.Value,
                        IsActive = true
                    };
                    _context.StudentClasses.Add(newRelation);
                }
                else if (currentRelation != null)
                {
                    currentRelation.IsActive = false;
                    _context.Update(currentRelation);
                }

                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(st);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound();
            student.IsActive = !student.IsActive;
            _context.Update(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}