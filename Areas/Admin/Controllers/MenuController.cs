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
    public class MenuController : Controller
    {
        private readonly DataContext _context;

        public MenuController(DataContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var mnList = _context.Menus.OrderBy(m => m.MenuID).ToList();
            return View(mnList);
        }

        public IActionResult Delete(int? id)
        {
           
            if (id == null || id == 0)
                return NotFound();
            var mn = _context.Menus.Find(id);
            if (mn == null)
                return NotFound();
            return View(mn);
        }
        [HttpPost]

        public IActionResult Delete(int id)
        {
            
            var delMenu = _context.Menus.Find(id);
            if (delMenu == null)
                return NotFound();
            _context.Menus.Remove(delMenu);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
          
            var mnList = (from m in _context.Menus
                          select new SelectListItem()
                          {
                              Text = (m.Levels == 1) ? m.MenuName : "-- " + m.MenuName,
                              Value = m.MenuID.ToString()
                          }).ToList();
            mnList.Insert(0, new SelectListItem()
            {
                Text = "--- select ---",
                Value = "0"
            });
            ViewBag.mnList = mnList;
            return View();
        }
        [HttpPost]
        public IActionResult Create(tblMenu mn)
        {
          
            if (ModelState.IsValid)
            {
                _context.Menus.Add(mn);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(mn);
        }
        public IActionResult Edit(int? id)
        {
             
            if (id == null || id == 0)
                return NotFound();
            var mn = _context.Menus.Find(id);
            if (mn == null)
                return NotFound();

            var mnList = (from m in _context.Menus
                          select new SelectListItem()
                          {
                              Text = (m.Levels == 1) ? m.MenuName : "--" + m.MenuName,
                              Value = m.MenuID.ToString()
                          }

            ).ToList();
            mnList.Insert(0, new SelectListItem()
            {
                Text = "---select---",
                Value = "0"
            }
            );
            ViewBag.mnList = mnList;
            return View(mn);

        }
        [HttpPost]
        public IActionResult Edit(tblMenu mn)
        {
           
            if (ModelState.IsValid)
            {
                _context.Menus.Update(mn);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(mn);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null)
                return NotFound();
            menu.IsActive = !menu.IsActive;
            _context.Update(menu);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}