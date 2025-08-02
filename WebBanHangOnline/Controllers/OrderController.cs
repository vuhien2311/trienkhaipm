using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHangOnline.Data;
using WebBanHangOnline.Models;
using WebBanHangOnline.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace WebBanHangOnline.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Hiển thị form thông tin đặt hàng
        /// </summary>
        public async Task<IActionResult> Checkout()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);
            var order = new Order
            {
                CustomerName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            };

            return View(order);
        }

        /// <summary>
        /// Xử lý việc đặt hàng từ form
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(Order checkoutInfo)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
            {
                ModelState.AddModelError("", "Giỏ hàng của bạn đang trống.");
            }

            ModelState.Remove("User");
            ModelState.Remove("OrderDetails");

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var finalOrder = new Order
                {
                    UserId = user.Id,
                    OrderDate = DateTime.UtcNow,
                    Status = "Chờ xử lý",
                    TotalAmount = cart.Sum(item => item.Total),
                    CustomerName = checkoutInfo.CustomerName,
                    Address = checkoutInfo.Address,
                    PhoneNumber = checkoutInfo.PhoneNumber,
                    Email = checkoutInfo.Email,
                    OrderDetails = new List<OrderDetail>()
                };

                // **BẮT ĐẦU NÂNG CẤP XỬ LÝ LỖI**
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // Thêm đơn hàng vào context
                        _context.Orders.Add(finalOrder);
                        // Lưu lại để lấy được OrderId cho các OrderDetail
                        await _context.SaveChangesAsync();

                        foreach (var item in cart)
                        {
                            var productInDb = await _context.Products.FindAsync(item.ProductId);
                            if (productInDb == null || productInDb.Stock < item.Quantity)
                            {
                                // Nếu sản phẩm không tồn tại hoặc không đủ hàng, hủy giao dịch
                                throw new Exception($"Sản phẩm '{item.ProductName}' không đủ hàng.");
                            }

                            // Giảm số lượng tồn kho
                            productInDb.Stock -= item.Quantity;

                            // Tạo chi tiết đơn hàng
                            var orderDetail = new OrderDetail
                            {
                                OrderId = finalOrder.Id, // Gán OrderId đã được tạo
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                Price = item.Price
                            };
                            _context.OrderDetails.Add(orderDetail);
                        }

                        // Lưu lại tất cả các thay đổi (cập nhật tồn kho và thêm chi tiết đơn hàng)
                        await _context.SaveChangesAsync();
                        // Nếu mọi thứ thành công, xác nhận giao dịch
                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        // Nếu có bất kỳ lỗi nào, hủy bỏ tất cả các thay đổi
                        await transaction.RollbackAsync();
                        // Ghi lại lỗi để hiển thị cho người dùng
                        ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi đặt hàng: " + ex.Message);
                        // Quay lại trang checkout với thông báo lỗi
                        return View("Checkout", checkoutInfo);
                    }
                }
                // **KẾT THÚC NÂNG CẤP**

                HttpContext.Session.Remove("Cart");
                TempData["SuccessMessage"] = "Bạn đã đặt hàng thành công!";
                return RedirectToAction("Success");
            }

            return View("Checkout", checkoutInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                                      .Include(o => o.OrderDetails)
                                      .ThenInclude(d => d.Product)
                                      .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null || order.UserId != user.Id)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction(nameof(MyOrders));
            }

            if (order.Status != "Chờ xử lý")
            {
                TempData["ErrorMessage"] = "Chỉ có thể hủy đơn hàng ở trạng thái 'Chờ xử lý'.";
                return RedirectToAction(nameof(MyOrders));
            }

            // Cập nhật trạng thái đơn hàng thành "Đã hủy"
            order.Status = "Đã hủy";

            // Hoàn lại số lượng tồn kho cho các sản phẩm
            foreach (var detail in order.OrderDetails)
            {
                var product = detail.Product;
                if (product != null)
                {
                    product.Stock += detail.Quantity;
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã hủy thành công đơn hàng #{id}.";
            return RedirectToAction(nameof(MyOrders));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                                      .Include(o => o.OrderDetails)
                                      .ThenInclude(d => d.Product)
                                      .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction(nameof(MyOrders));
            }

            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();

            foreach (var detail in order.OrderDetails)
            {
                var product = detail.Product;
                if (product != null && product.Stock > 0)
                {
                    var cartItem = cart.FirstOrDefault(ci => ci.ProductId == product.Id);
                    if (cartItem != null)
                    {
                        // Nếu sản phẩm đã có trong giỏ, cộng thêm số lượng
                        cartItem.Quantity += detail.Quantity;
                    }
                    else
                    {
                        // Nếu chưa có, thêm mới vào giỏ
                        cart.Add(new CartItem
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            Price = product.Price,
                            Quantity = detail.Quantity,
                            ImageUrl = product.ImageUrl
                        });
                    }
                }
            }

            HttpContext.Session.Set("Cart", cart);
            TempData["SuccessMessage"] = "Đã thêm các sản phẩm từ đơn hàng cũ vào giỏ hàng!";
            return RedirectToAction("Index", "Cart");
        }

        public IActionResult Success()
        {
            return View();
        }

        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            var orders = await _context.Orders
                                         // THÊM 2 DÒNG NÀY VÀO
                                         .Include(o => o.OrderDetails)
                                         .ThenInclude(d => d.Product)
                                         .Where(o => o.UserId == user.Id)
                                         .OrderByDescending(o => o.OrderDate)
                                         .ToListAsync();
            return View(orders);
        }
    }
}
