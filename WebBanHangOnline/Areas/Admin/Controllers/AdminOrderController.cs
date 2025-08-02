using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHangOnline.Data;
using System.Linq;
using System.Threading.Tasks;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminOrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminOrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/AdminOrder
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders.Include(o => o.User).OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(orders);
        }

        // GET: Admin/AdminOrder/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // POST: Admin/AdminOrder/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            if (!string.IsNullOrEmpty(status))
            {
                order.Status = status;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
