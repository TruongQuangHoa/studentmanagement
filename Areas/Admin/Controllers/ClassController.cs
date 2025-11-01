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
                // Tính năm học dựa theo niên khóa
                var grade = _context.Grades.Find(_class.GradeID);
                if (grade != null)
                {
                    int yearOffset = grade.GradeID - 1; // hoặc tùy theo ID thực tế
                    _class.SchoolYear = $"{cohort.StartYear + yearOffset}-{cohort.StartYear + yearOffset + 1}";
                }

                bool exists = _context.Classes.Any(l =>
                    l.ClassName == _class.ClassName &&
                    l.CohortID == _class.CohortID &&
                    l.SchoolYear == _class.SchoolYear);

                if (!exists)
                {
                    _context.Classes.Add(_class);
                    _context.SaveChanges();
                }
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
            var grades = _context.Grades
                .OrderBy(k => k.GradeID)
                .Select(k => new { k.GradeID, k.GradeName })
                .ToList();

            // Thêm dòng mặc định ở đầu danh sách
            grades.Insert(0, new { GradeID = 0, GradeName = "---- Khối ----" });

            ViewBag.gdList = new SelectList(grades, "GradeID", "GradeName", _class?.GradeID);

            // Lấy danh sách khóa học (niên khóa)
            var chList = _context.Cohorts
                .Where(c => c.IsActive)
                .Select(c => new { c.CohortID, ThongTin = c.StartYear + "-" + c.EndYear + " - Khóa " + c.CohortName })
                .ToList();

            ViewBag.chList = new SelectList(chList, "CohortID", "ThongTin", _class?.CohortID);
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
    }
}