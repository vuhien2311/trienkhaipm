using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHangOnline.Data;
using System.Linq;
using System.Threading.Tasks;

namespace WebBanHangOnline.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                                         .Include(p => p.Category)
                                         .OrderByDescending(p => p.Id)
                                         .Take(8)
                                         .ToListAsync();
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
