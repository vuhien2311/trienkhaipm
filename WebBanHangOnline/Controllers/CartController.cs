using Microsoft.AspNetCore.Mvc;
using WebBanHangOnline.Models;
using WebBanHangOnline.Extensions;
using System.Collections.Generic;
using WebBanHangOnline.Data;
using System.Linq;
using System.Threading.Tasks;

namespace WebBanHangOnline.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "Cart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        private List<CartItem> GetCart()
        {
            return HttpContext.Session.Get<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.Set(CartSessionKey, cart);
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var cart = GetCart();
            var cartItem = cart.Find(p => p.ProductId == id);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });
            }

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            var cartItem = cart.Find(p => p.ProductId == id);

            if (cartItem != null)
            {
                cart.Remove(cartItem);
                SaveCart(cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateCart(int id, int quantity)
        {
            var cart = GetCart();
            var cartItem = cart.Find(p => p.ProductId == id);

            if (cartItem != null)
            {
                if (quantity > 0)
                {
                    cartItem.Quantity = quantity;
                }
                else
                {
                    cart.Remove(cartItem);
                }
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }
    }
}
