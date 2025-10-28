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
    public class StudentController : Controller
    {
        private readonly DataContext _context;

        public StudentController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var stList = _context.Students.OrderBy(m => m.StudentClassID).ToList();
            return View(stList);
        }

        public IActionResult Delete(int? id)
        {
           
            if (id == null || id == 0)
                return NotFound();
            var st = _context.Students.Find(id);
            if (st == null)
                return NotFound();
            return View(st);
        }
        [HttpPost]

        public IActionResult Delete(int id)
        {
            var delStudent = _context.Students.Find(id);
            if (delStudent == null)
                return NotFound();
            _context.Students.Remove(delStudent);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
          
            var stList = (from s in _context.Students
                          select new SelectListItem()
                          {
                              Text = s.StudentID,
                              Value = s.StudentClassID.ToString()
                          }).ToList();
            stList.Insert(0, new SelectListItem()
            {
                Text = "--- select ---",
                Value = "0"
            });
            ViewBag.stList = stList;
            return View();
        }
        [HttpPost]
        public IActionResult Create(tblStudent st)
        {
          
            if (ModelState.IsValid)
            {
                _context.Students.Add(st);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(st);
        }
        public IActionResult Edit(int? id)
        {
             
            if (id == null || id == 0)
                return NotFound();
            var st = _context.Students.Find(id);
            if (st == null)
                return NotFound();

            var stList = (from s in _context.Students
                          select new SelectListItem()
                          {
                              Text = s.StudentID,
                              Value = s.StudentClassID.ToString()
                          }

            ).ToList();
            stList.Insert(0, new SelectListItem()
            {
                Text = "---select---",
                Value = "0"
            }
            );
            ViewBag.stList = stList;
            return View(st);

        }
        [HttpPost]
        public IActionResult Edit(tblStudent st)
        {
           
            if (ModelState.IsValid)
            {
                _context.Students.Update(st);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(st);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound();
            student.IsActive = !student.IsActive;
            _context.Update(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}