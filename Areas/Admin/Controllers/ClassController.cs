using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
//using StudentManagement.Ultilities;
using StudentManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
        public IActionResult Index()
        {
            // ViewBag.IsAdmin = Functions.IsAdmin(HttpContext);
            // ViewBag.IsTeacher = Functions.IsTeacher(HttpContext);

            var clList = _context.Classes
                .Include(l => l.grade)
                .Include(l => l.cohort)
                .OrderBy(l => l.ClassID)
                .ToList()
                .Select(l =>
                {
                    if (string.IsNullOrEmpty(l.SchoolYear) && l.cohort != null)
                        l.SchoolYear = ComputeSchoolYear(l);
                    return l;
                })
                .ToList();

            return View(clList);
        }

        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        public IActionResult Create(tblClass _class)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(_class);
                return View(_class);
            }

            var cohort = _context.Cohorts.Find(_class.CohortID);
            if (cohort != null && cohort.StartYear.HasValue && cohort.EndYear.HasValue)
            {
                var gdList = _context.Grades.OrderBy(k => k.GradeID).ToList();
                int totalYears = Math.Min(gdList.Count, cohort.EndYear.Value - cohort.StartYear.Value);

                for (int i = 0; i < totalYears; i++)
                {
                    var classNew = new tblClass
                    {
                        ClassName = _class.ClassName,
                        MaxStudents = _class.MaxStudents,
                        CurrentStudents = 0,
                        IsActive = _class.IsActive,
                        CohortID = _class.CohortID,
                        GradeID = gdList[i].GradeID,
                        SchoolYear = $"{cohort.StartYear + i}-{cohort.StartYear + i + 1}"
                    };

                    // Tránh trùng lớp cùng tên + khóa + SchoolYear
                    bool exists = _context.Classes.Any(l =>
                        l.ClassName == _class.ClassName &&
                        l.CohortID == _class.CohortID &&
                        l.SchoolYear == _class.SchoolYear);

                    if (!exists)
                        _context.Classes.Add(classNew);
                }
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int? id)
        {
            if (!id.HasValue) return NotFound();

            var _class = _context.Classes
                .Include(l => l.grade)
                .Include(l => l.cohort)
                .FirstOrDefault(l => l.ClassID == id);

            if (_class == null) return NotFound();

            LoadDropdowns(_class);
            return View(_class);
        }

        [HttpPost]
        public IActionResult Edit(tblClass _class)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(_class);
                return View(_class);
            }

            var cohort = _context.Cohorts.Find(_class.CohortID);

            // Chỉ tính SchoolYear nếu chưa có (giữ thủ công nếu đã nhập)
            if (string.IsNullOrEmpty(_class.SchoolYear) && cohort != null && cohort.StartYear.HasValue)
                _class.SchoolYear = ComputeSchoolYear(_class);

            _context.Update(_class);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int? id)
        {
            if (!id.HasValue) return NotFound();

            var _class = _context.Classes.Find(id);
            if (_class == null) return NotFound();

            return View(_class);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var _class = _context.Classes.Find(id);
            if (_class != null)
            {
                _context.Classes.Remove(_class);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var _class = await _context.Classes.FindAsync(id);
            if (_class != null)
            {
                _class.IsActive = !_class.IsActive;
                _context.Update(_class);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private void LoadDropdowns(tblClass _class = null)
        {
            // Lấy danh sách khối
            ViewBag.gdList = new SelectList(_context.Grades.OrderBy(k => k.GradeID),
                "GradeID", "GradeName", _class?.GradeID);

            // Lấy danh sách khóa học (niên khóa)
            var chList = _context.Cohorts
                .Where(c => c.IsActive)
                .Select(c => new { c.CohortID, Info = c.StartYear + "-" + c.EndYear + " - Khóa " + c.CohortName })
                .ToList();

            ViewBag.chList = new SelectList(chList, "CohortID", "Info", _class?.CohortID);
        }


        private string ComputeSchoolYear(tblClass _class)
        {
            if (_class.cohort == null || !_class.cohort.StartYear.HasValue) return null;

            var gdList = _context.Grades.OrderBy(k => k.GradeID).ToList();
            int index = gdList.FindIndex(k => k.GradeID == _class.GradeID);
            if (index < 0) index = 0;

            int startYear = _class.cohort.StartYear.Value + index;
            return $"{startYear}-{startYear + 1}";
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