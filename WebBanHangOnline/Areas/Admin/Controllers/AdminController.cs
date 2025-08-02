using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHangOnline.Data;
using WebBanHangOnline.Models;
using System.Linq;
using System.Threading.Tasks;

namespace WebBanHangOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Dữ liệu cho các thẻ thống kê
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();

            var today = DateTime.UtcNow.Date;
            ViewBag.OrdersToday = await _context.Orders.CountAsync(o => o.OrderDate.Date == today);
            ViewBag.RevenueToday = await _context.Orders
                                                 .Where(o => o.OrderDate.Date == today && o.Status == "Hoàn thành")
                                                 .SumAsync(o => (double?)o.TotalAmount) ?? 0;

            // Lấy 5 đơn hàng gần đây nhất để hiển thị
            var recentOrders = await _context.Orders
                                             .OrderByDescending(o => o.OrderDate)
                                             .Take(5)
                                             .ToListAsync();

            return View(recentOrders); // Truyền danh sách đơn hàng gần đây làm Model cho View
        }
    }
}
