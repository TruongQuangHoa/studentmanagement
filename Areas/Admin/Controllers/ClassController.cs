using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace StudentManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ClassController : Controller
    {
        private readonly DataContext _context;

        public ClassController(DataContext context)
        {
            _context = context;
        }

        // GET: Index
        public IActionResult Index()
        {
            var classList = _context.Classes
                .OrderBy(c => c.GradeID)
                .ThenBy(c => c.ClassID)
                .ToList();

            foreach (var cls in classList)
            {
                cls.grade = _context.Grades.FirstOrDefault(g => g.GradeID == cls.GradeID);
                cls.cohort = _context.Cohorts.FirstOrDefault(c => c.CohortID == cls.CohortID);
            }

            return View(classList);
        }

        // GET: Create
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(tblClass _class)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return View(_class);
            }

            var cohort = _context.Cohorts.FirstOrDefault(c => c.CohortID == _class.CohortID);
            if (cohort == null)
            {
                ModelState.AddModelError("", "Chưa chọn khóa học hợp lệ.");
                LoadDropdowns();
                return View(_class);
            }

            var grades = _context.Grades
                        .Where(g => g.IsActive && g.GradeName != null)
                        .OrderBy(g => g.GradeID)
                        .Take(4)
                        .ToList();

            var classList = new List<tblClass>();

            for (int i = 0; i < grades.Count; i++)
            {
                var grade = grades[i];
                int startYear = cohort.StartYear.Value + i;

                var newClass = new tblClass
                {
                    ClassName = _class.ClassName,
                    GradeID = grade.GradeID,
                    CohortID = _class.CohortID,
                    MaxStudents = _class.MaxStudents,
                    CurrentStudents = 0,
                    IsActive = _class.IsActive,
                    SchoolYear = $"{startYear}-{startYear + 1}",
                    cohort = cohort,
                    grade = grade
                };

                classList.Add(newClass);
            }

            _context.Classes.AddRange(classList);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Edit
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var cls = _context.Classes.Find(id);
            if (cls == null) return NotFound();
            cls.grade = _context.Grades.FirstOrDefault(g => g.GradeID == cls.GradeID);
            cls.cohort = _context.Cohorts.FirstOrDefault(c => c.CohortID == cls.CohortID);
            return View(cls);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(tblClass model)
        {
            if (!ModelState.IsValid) return View(model);

            var cls = _context.Classes.Find(model.ClassID);
            if (cls == null) return NotFound();

            cls.ClassName = model.ClassName;
            cls.MaxStudents = model.MaxStudents;
            cls.CurrentStudents = model.CurrentStudents;
            cls.IsActive = model.IsActive;

            // Không sửa GradeID và CohortID
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Delete
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var classItem = _context.Classes
                .Include(c => c.grade)
                .Include(c => c.cohort)
                .FirstOrDefault(c => c.ClassID == id);

            if (classItem == null)
                return NotFound();

            return View(classItem);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var classItem = _context.Classes.FirstOrDefault(c => c.ClassID == id);
            if (classItem == null)
                return NotFound();

            _context.Classes.Remove(classItem);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var _class = _context.Classes.FirstOrDefault(c => c.ClassID == id);
            if (_class != null)
            {
                _class.IsActive = !_class.IsActive;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        private void LoadDropdowns()
        {
            var chList = _context.Cohorts
                .Where(c => c.IsActive)
                .OrderBy(c => c.StartYear)
                .Select(c => new
                {
                    c.CohortID,
                    Display = c.StartYear + "-" + c.EndYear + " - Khóa " + c.CohortName
                })
                .ToList();

            ViewBag.chList = new SelectList(chList, "CohortID", "Display");
        }

        // Thống kê
    //     public IActionResult ThongKe()
    //     {
    //         // Tổng thống kê điểm toàn khối (coi như toàn trường)
    //         var allGrades = _context.QLDiems.ToList();
    //         var totalExams = allGrades.Count;
    //         var sumOfAllScores = allGrades.Sum(d => d.AverageScore);
    //         var averageScoreAll = totalExams > 0 ? (double)sumOfAllScores / totalExams : 0;

    //         // Thống kê điểm theo từng khối (for dropdown)
    //         var gradeLevelStats = _context.QLKhois.Select(k => k.GradeName).Distinct().ToList();

    //         // Thống kê điểm trung bình theo từng lớp (for dropdown)
    //         var classStats = _context.QLLopHocs.Select(l => l.ClassName).Distinct().ToList();

    //         ViewBag.AverageScoreAll = averageScoreAll;
    //         ViewBag.GradeLevels = gradeLevelStats;
    //         ViewBag.Classes = classStats;
    //         ViewBag.TotalExams = totalExams;



    //         return View();
    //     }


    //     public IActionResult DiemTheoKhoiPartial(string gradeLevelFilter)
    //     {
    //         if (string.IsNullOrWhiteSpace(gradeLevelFilter) || gradeLevelFilter == "all")
    //         {
    //             return Json(new List<object> { new {
    //         GradeName = "Toàn trường",
    //         AverageScore = _context.QLDiems.Average(d => (double?)d.AverageScore) ?? 0
    //     }});
    //         }

    //         var gradeAvg = _context.QLDiems
    //  .Join(_context.QLMonHocs, d => d.SubjectID, m => m.SubjectID, (d, m) => new { d, m })
    //  .GroupBy(x => x.m.SubjectName) // Nhóm theo tên môn học
    //  .Select(g => new
    //  {
    //      SubjectName = g.Key,
    //      AverageScore = g.Average(x => x.d.AverageScore)
    //  })
    //  .ToList();

    //         return Json(gradeAvg);
    //     }

    //     public IActionResult DiemTrungBinhTheoLopPartial(string classFilter)
    //     {
    //         if (string.IsNullOrWhiteSpace(classFilter) || classFilter == "all")
    //         {
    //             return Json(new List<object> { new {
    //         ClassName = "Tất cả lớp",
    //         AverageScore = _context.QLDiems.Average(d => (double?)d.AverageScore) ?? 0
    //     }});
    //         }

    //         var classAvg = _context.QLDiems
    //   .Join(_context.QLHocSinhs, d => d.StudentID, hs => hs.StudentID, (d, hs) => new { d, hs })
    //   .Join(_context.QLHocSinhLopHocs.Where(hsl => hsl.IsActive), x => x.hs.StudentID, hsl => hsl.StudentID, (x, hsl) => new { x.d, hsl })
    //   .Join(_context.QLLopHocs, x => x.hsl.ClassID, lh => lh.ClassID, (x, lh) => new { x.d, lh })
    //   .Where(x => x.lh.ClassName == classFilter)
    //   .GroupBy(x => x.lh.ClassName)
    //   .Select(g => new
    //   {
    //       ClassName = g.Key,
    //       AverageScore = g.Average(x => x.d.AverageScore)
    //   })
    //   .ToList();

    //         return Json(classAvg);

    //     }
    //     public IActionResult DiemTrungBinhHocSinhPartial(string studentCodeFilter)
    //     {
    //         try
    //         {
    //             if (string.IsNullOrWhiteSpace(studentCodeFilter))
    //                 return Content(string.Empty);

    //             var filter = studentCodeFilter.Trim().ToLower();

    //             var studentScores = _context.QLDiems
    // .Join(_context.QLHocSinhs,
    //       d => d.StudentID.ToString(),
    //       hs => hs.StudentID.ToString(),
    //       (d, hs) => new { d, hs })
    // .Where(x =>
    //     x.hs.StudentID.ToString().ToLower().Contains(filter) ||
    //     x.hs.FullName.ToLower().Contains(filter))
    // .Join(_context.QLMonHocs,
    //       x => x.d.SubjectID,
    //       m => m.SubjectID,
    //       (x, m) => new { x.hs, x.d, m })
    // // Join bảng trung gian QLHocSinhLopHoc
    // .Join(_context.QLHocSinhLopHocs.Where(hsl => hsl.IsActive),
    //       temp => temp.hs.StudentID,
    //       hsl => hsl.StudentID,
    //       (temp, hsl) => new { temp.hs, temp.d, temp.m, hsl })
    // // Join lớp học
    // .Join(_context.QLLopHocs,
    //       temp => temp.hsl.ClassID,
    //       lop => lop.ClassID,
    //       (temp, lop) => new
    //       {
    //           temp.hs.StudentID,
    //           temp.hs.FullName,
    //           temp.m.SubjectName,
    //           temp.d.AverageScore,
    //           ClassName = lop.ClassName
    //       })
    // .GroupBy(x => new { x.StudentID, x.FullName, x.SubjectName, x.ClassName })
    // .Select(g => new
    // {
    //     StudentCode = g.Key.StudentID.ToString(),
    //     FullName = g.Key.FullName,
    //     SubjectName = g.Key.SubjectName,
    //     ClassName = g.Key.ClassName,
    //     AverageScore = g.Average(x => x.AverageScore)
    // })
    // .ToList();


    //             if (!studentScores.Any())
    //                 return Content(string.Empty);

    //             return Json(studentScores);
    //         }
    //         catch (Exception ex)
    //         {
    //             return Content("Lỗi xử lý controller: " + ex.Message);
    //         }
    //     }
    }
}
