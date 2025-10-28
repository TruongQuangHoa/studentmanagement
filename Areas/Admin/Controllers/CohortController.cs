using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StudentManagement.Models;

namespace StudentManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CohortController : Controller
    {
        private readonly DataContext _context;

        public CohortController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var chList = _context.Cohorts.OrderBy(c => c.CohortID).ToList();
            return View(chList);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var ch = _context.Cohorts.Find(id);
            if (ch == null)
                return NotFound();
            return View(ch);
        }
        [HttpPost]

        public IActionResult Delete(int id)
        {
            var delCohort = _context.Cohorts.Find(id);
            if (delCohort == null)
                return NotFound();
            _context.Cohorts.Remove(delCohort);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(tblCohort ch)
        {

            if (ModelState.IsValid)
            {
                _context.Cohorts.Add(ch);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(ch);
        }
    
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var cohort = _context.Cohorts.Find(id);
            if (cohort == null)
                return NotFound();
            return View(cohort);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(tblCohort ch)
        {
            if (!ModelState.IsValid)
                return View(ch);

            var existing = _context.Cohorts.Find(ch.CohortID);
            if (existing == null)
                return NotFound();

            existing.StartYear = ch.StartYear;
            existing.EndYear = ch.EndYear;

            // Niên khóa tự động dựa trên StartYear so với mốc 2024 → 60
            existing.CohortName = 60 + (ch.StartYear - 2024);

            existing.IsActive = ch.IsActive;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}