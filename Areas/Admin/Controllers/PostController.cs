using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;
using PagedList.Core;

namespace StudentManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PostController : Controller
    {
        private readonly DataContext _context;

        public PostController(DataContext context)
        {
            _context = context;
        }
        
        public IActionResult Index()
        {
            int page = 1;
            var post = _context.Posts.OrderBy(p => p.PostID);
            int pageSize = 5; // Số bài viết trên 1 trang;
            PagedList<tblPost> models = new(post, page, pageSize);
            if (models == null)
               return NotFound();
            return View(models);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(tblPost pt)
        {
            if (ModelState.IsValid)
            {
                _context.Posts.Add(pt);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Edit(long? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var pc = _context.Posts.Find(id);
            if (pc == null)
                return NotFound();
            return View(pc);
        }
        [HttpPost]
        public IActionResult Edit(tblPost pt)
        {
            if (ModelState.IsValid)
            {
                _context.Update(pt);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(pt);
        }

        public IActionResult Delete(long? id)
        {
            if (id == null || id == 0)
                return NotFound();
            var pc = _context.Posts.Find(id);
            if (pc == null)
                return NotFound();
            return View(pc);
        }
        [HttpPost]
        public IActionResult Delete(long id)
        {
            var delPT = _context.Posts.Find(id);
            if (delPT == null)
                return NotFound();
            _context.Posts.Remove(delPT);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(long id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound();
            post.IsActive = !post.IsActive;
            _context.Update(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}