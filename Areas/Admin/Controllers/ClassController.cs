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
                        GradeID = gdList[i].GradeID,
                        NumberOfStudent = _class.NumberOfStudent,
                        SchoolYear = $"{cohort.StartYear + i}-{cohort.StartYear + i + 1}",
                        IsActive = _class.IsActive,
                        CohortID = _class.CohortID
                    };

                    bool exists = _context.Classes.Any(l =>
                        l.ClassName == classNew.ClassName &&
                        l.CohortID == classNew.CohortID &&
                        l.SchoolYear == classNew.SchoolYear);

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
            ViewBag.gdList = new SelectList(_context.Grades.OrderBy(k => k.GradeID),
                "GradeID", "GradeName", _class?.GradeID);

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