using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using StudentManagement.Models;

namespace StudentManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GradeController : Controller
    {
        private readonly DataContext _context;

        public GradeController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var gdList = _context.Grades.OrderBy(g => g.GradeID).ToList();
            return View(gdList);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var gd = _context.Grades.Find(id);
            if (gd == null)
                return NotFound();
            return View(gd);
        }
        [HttpPost]

        public IActionResult Delete(int id)
        {
            var delGrade = _context.Grades.Find(id);
            if (delGrade == null)
                return NotFound();
            _context.Grades.Remove(delGrade);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            var gdList = (from g in _context.Grades
                          select new SelectListItem()
                          {
                              Text = (g.GradeName != null) ? g.GradeName : "-- " + g.GradeName,
                              Value = g.GradeID.ToString()
                          }).ToList();
            gdList.Insert(0, new SelectListItem()
            {
                Text = "--- select ---",
                Value = "0"
            });
            ViewBag.gdList = gdList;
            return View();
        }
        [HttpPost]
        public IActionResult Create(tblGrade gd)
        {
            if (ModelState.IsValid)
            {
                _context.Grades.Add(gd);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(gd);
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var gd = _context.Grades.Find(id);
            if (gd == null)
                return NotFound();

            var gdList = (from g in _context.Grades
                          select new SelectListItem()
                          {
                              Text = (g.GradeName != null) ? g.GradeName : "--" + g.GradeName,
                              Value = g.GradeID.ToString()
                          }

            ).ToList();
            gdList.Insert(0, new SelectListItem()
            {
                Text = "---select---",
                Value = "0"
            }
            );
            ViewBag.gdList = gdList;
            return View(gd);

        }
        [HttpPost]
        public IActionResult Edit(tblGrade gd)
        {
            if (ModelState.IsValid)
            {
                _context.Grades.Update(gd);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(gd);
        }
        
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var grade = await _context.Grades.FindAsync(id);
            if (grade == null)
                return NotFound();


            grade.IsActive = !grade.IsActive;
            _context.Update(grade);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}