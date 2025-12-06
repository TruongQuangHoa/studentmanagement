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
using Microsoft.AspNetCore.Authorization;

namespace StudentManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Bắt buộc: Chỉ Admin mới được vào
    public class SubjectController : Controller
    {
        private readonly DataContext _context;
        public SubjectController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var sbList = _context.Subjects.OrderBy(m => m.SubjectID)
                .Include(m => m.department)
                .ToList();

            return View(sbList);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var sb = _context.Subjects.Find(id);
            if (sb == null)
                return NotFound();
            return View(sb);
        }
        [HttpPost]

        public IActionResult Delete(int id)
        {
            var delsubject = _context.Subjects.Find(id);
            if (delsubject == null)
                return NotFound();
            _context.Subjects.Remove(delsubject);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            LoadData();
            return View();
        }
        [HttpPost]
        public IActionResult Create(tblSubject sb)
        {
            if (ModelState.IsValid)
            {
                _context.Subjects.Add(sb);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            LoadData();
            return View(sb);
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var mn = _context.Subjects.Find(id);
            if (mn == null)
                return NotFound();
            LoadData();
            return View(mn);

        }
        [HttpPost]
        public IActionResult Edit(tblSubject sb)
        {
            if (ModelState.IsValid)
            {
                _context.Subjects.Update(sb);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            LoadData();
            return View(sb);
        }
        private void LoadData()
        {
            ViewBag.gdlist = new SelectList(_context.Grades, "GradeID", "GradeName");
            var dpList = _context.Departments.Where(mon => mon.IsActive == true)
          .Select(dp => new
          {
              dp.DepartmentID,
              Info = dp.DepartmentID + " - " + dp.DepartmentName
          }).ToList();
            ViewBag.dpList = new SelectList(dpList, "DepartmentID", "Info");

        }
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
                return NotFound();
            subject.IsActive = !subject.IsActive;
            _context.Update(subject);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


    }
}