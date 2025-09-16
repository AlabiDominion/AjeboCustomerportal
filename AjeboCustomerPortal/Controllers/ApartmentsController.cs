using System;
using System.Linq;
using System.Threading.Tasks;
using AjeboCustomerPortal.Data;
using AjeboCustomerPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AjeboCustomerPortal.Controllers
{
    public class ApartmentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ApartmentsController(ApplicationDbContext db) => _db = db;

        // GET: /Apartments
        public async Task<IActionResult> Index()
        {
            var apartments = await _db.Apartments
                .OrderByDescending(a => a.Id)
                .ToListAsync();
            return View(apartments);
        }

        // GET: /Apartments/Details/5
        public async Task<IActionResult> Detailes(int id)
        {
            var apt = await _db.Apartments
                .Include(a => a.Reviews)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (apt == null) return NotFound();
            return View(apt);
        }

        // POST: /Apartments/AddToCart
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(AddToCartVm vm)
        {
            if (!ModelState.IsValid) return BadRequest();
            BookingGuard.EnsureValidDates(vm.StartDate, vm.EndDate);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

            // get or create open cart
            var cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsCheckedOut);
            if (cart == null)
            {
                cart = new Cart { UserId = userId, IsCheckedOut = false };
                _db.Carts.Add(cart);
            }

            // merge same apartment/date range lines
            var existing = cart.Items.FirstOrDefault(i =>
                i.ApartmentId == vm.ApartmentId &&
                i.StartDate.Date == vm.StartDate.Date &&
                i.EndDate.Date == vm.EndDate.Date);

            if (existing == null)
            {
                var apt = await _db.Apartments.FirstOrDefaultAsync(a => a.Id == vm.ApartmentId);
                if (apt == null) return NotFound();

                cart.Items.Add(new CartItem
                {
                    ApartmentId = vm.ApartmentId,
                    StartDate = vm.StartDate.Date,
                    EndDate = vm.EndDate.Date,
                    Quantity = 1
                });
            }
            else
            {
                existing.Quantity += 1; // or keep 1 for rentals
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "Cart");
        }
    }

    // ViewModel used by AddToCart
    public class AddToCartVm
    {
        public int ApartmentId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    // Simple date guard helpers
    public static class BookingGuard
    {
        public static void EnsureValidDates(DateTime start, DateTime end)
        {
            if (start.Date >= end.Date)
                throw new InvalidOperationException("End date must be after start date.");
            if (start.Date < DateTime.UtcNow.Date)
                throw new InvalidOperationException("Start date cannot be in the past.");
        }
        public static int Nights(DateTime s, DateTime e) => (e.Date - s.Date).Days;
    }
}