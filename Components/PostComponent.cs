using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StudentManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace supersportvn.Components
{
    [ViewComponent(Name = "Post")]
    public class PostComponent : ViewComponent
    {
        private readonly DataContext _context;
        public PostComponent(DataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var listPost = (from p in _context.Posts
                                where (p.IsActive == true)
                                orderby p.PostID descending
                                select p).Take(1).ToList();
            return await Task.FromResult((IViewComponentResult)View("Default", listPost));
        }
    }
}