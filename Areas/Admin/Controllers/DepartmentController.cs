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
    public class DepartmentController : Controller
    {
        private readonly DataContext _context;

        public DepartmentController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var scheduleList = _context.Departments
                .OrderBy(s => s.DepartmentID)
                .ToList();
            return View(scheduleList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(tblDepartment model)
        {
            if (ModelState.IsValid)
            {
                _context.Departments.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var schedule = _context.Departments.Find(id);
            if (schedule == null)
                return NotFound();
            return View(schedule);
        }

        [HttpPost]
        public IActionResult Edit(tblDepartment model)
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

            var schedule = _context.Departments.Find(id);
            if (schedule == null)
                return NotFound();

            return View(schedule);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var schedule = _context.Departments.Find(id);
            if (schedule == null)
                return NotFound();

            _context.Departments.Remove(schedule);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var dp = await _context.Departments.FindAsync(id);
            if (dp == null)
                return NotFound();

            dp.IsActive = !dp.IsActive;

            _context.Update(dp);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}