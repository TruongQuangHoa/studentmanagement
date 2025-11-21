using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore; 
using System.Threading.Tasks;

namespace StudentManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DataContext _context;

    public HomeController(ILogger<HomeController> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        
        var featuredPosts = await _context.tblPost 
                                         .Where(p => p.IsActive == true && p.IsFeatured == true)
                                         .OrderBy(p => p.PostOrder)
                                         .ToListAsync();

        
        var supportContacts = new List<SupportContact>(); 
        
        
        var viewModel = new HomeDashboardVM
        {
            FullName = "Nguyễn Bảo Long", 
            FeaturedPosts = featuredPosts, 
            SupportContacts = supportContacts 
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}