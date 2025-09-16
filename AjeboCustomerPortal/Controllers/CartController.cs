using System.Linq;
using System.Threading.Tasks;
using AjeboCustomerPortal.Data;
using AjeboCustomerPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AjeboCustomerPortal.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CartController(ApplicationDbContext db) => _db = db;

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            var cart = await _db.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Apartment)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut);
            return View(cart);
        }

        // POST: /Cart/RemoveItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            var item = await _db.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == id && ci.Cart.UserId == userId && !ci.Cart.IsCheckedOut);
            if (item == null) return NotFound();
            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: /Cart/Checkout
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
            var cart = await _db.Carts
                .Include(c => c.Items).ThenInclude(i => i.Apartment)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut);
            if (cart == null || !cart.Items.Any()) return RedirectToAction("Index");

            // Re‑validate dates quickly
            foreach (var i in cart.Items)
                AjeboCustomerPortal.Controllers.BookingGuard.EnsureValidDates(i.StartDate, i.EndDate);

            // Create Order snapshot
            var order = new Order
            {
                UserId = userId,
                Status = "Pending",
                Items = cart.Items.Select(i => new OrderItem
                {
                    ApartmentId = i.ApartmentId,
                    UnitPrice = i.Apartment.Price, // snapshot
                    Quantity = i.Quantity,
                    StartDate = i.StartDate,
                    EndDate = i.EndDate
                }).ToList()
            };
            order.TotalAmount = order.Items.Sum(oi =>
                AjeboCustomerPortal.Controllers.BookingGuard.Nights(oi.StartDate, oi.EndDate) * oi.UnitPrice * oi.Quantity);

            _db.Orders.Add(order);
            cart.IsCheckedOut = true; // lock the cart
            await _db.SaveChangesAsync();

            return RedirectToAction("Pay", "Payments", new { orderId = order.Id });
        }
    }
}