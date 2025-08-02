using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHangOnline.Data;
using System.Linq;
using System.Threading.Tasks;

namespace WebBanHangOnline.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> List(string? searchString, int? categoryId)
        {
            // Lấy query cơ bản
            var productsQuery = _context.Products.Include(p => p.Category).AsQueryable();

            // Lọc theo từ khóa tìm kiếm nếu có
            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchString));
            }

            // Lọc theo danh mục nếu có
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await productsQuery.ToListAsync();

            // Gửi các giá trị lọc về View để hiển thị lại
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentCategory"] = categoryId;

            return View(products);
        }

        public IActionResult Details(int id)
        {
            TempData["SuccessMessage"] = "Tính năng xem chi tiết sản phẩm đang được phát triển. Sản phẩm đã được thêm vào giỏ hàng!";
            return RedirectToAction("AddToCart", "Cart", new { id = id, quantity = 1 });
        }
    }
}
