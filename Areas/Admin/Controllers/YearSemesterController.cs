using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using StudentManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace StudentManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Bắt buộc: Chỉ Admin mới được vào
    public class YearSemesterController : Controller
    {
        private readonly DataContext _context;

        public YearSemesterController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {     
            var ysList = _context.YearSemesters
                .OrderBy(s => s.YearSemesterID)
                .ToList();
            return View(ysList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(tblYearSemester model)
        {
            if (ModelState.IsValid)
            {
                _context.YearSemesters.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var schedule = _context.YearSemesters.Find(id);
            if (schedule == null)
                return NotFound();
            return View(schedule);
        }

        [HttpPost]
        public IActionResult Edit(tblYearSemester model)
        {
            if (ModelState.IsValid)
            {
                _context.Update(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var schedule = _context.YearSemesters.Find(id);
            if (schedule == null)
                return NotFound();

            return View(schedule);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var schedule = _context.YearSemesters.Find(id);
            if (schedule == null)
                return NotFound();

            _context.YearSemesters.Remove(schedule);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
         [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var yearsemester = await _context.YearSemesters.FindAsync(id);
            if (yearsemester == null)
                return NotFound();
            yearsemester.IsActive = !yearsemester.IsActive;
            _context.Update(yearsemester);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}