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
    }
}
