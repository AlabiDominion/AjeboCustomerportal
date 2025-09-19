using AjeboCustomerPortal.Data;
using AjeboCustomerPortal.Models;
using AjeboCustomerPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using AjeboCustomerPortal.ViewModels;

namespace AjeboCustomerPortal.Controllers
{
    public class ApartmentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ApartmentsController(ApplicationDbContext db) => _db = db;


public async Task<IActionResult> Index([FromQuery] ApartmentFilterVm f)
    {
        // Base query
        var q = _db.Apartments.AsQueryable();

        // Location
        if (!string.IsNullOrWhiteSpace(f.Location))
            q = q.Where(a => a.City == f.Location);

        // Budget per night
        if (f.MinBudget.HasValue) q = q.Where(a => a.Price >= f.MinBudget.Value);
        if (f.MaxBudget.HasValue) q = q.Where(a => a.Price <= f.MaxBudget.Value);

        // Guests (simple proxy: bedrooms >= guests)
        if (f.Guests.HasValue && f.Guests.Value > 0)
            q = q.Where(a => a.Bedrooms >= f.Guests.Value);

        // Availability (exclude apartments with PAID overlaps in the requested range)
        if (f.CheckIn.HasValue && f.CheckOut.HasValue && f.CheckIn < f.CheckOut)
        {
            var start = f.CheckIn.Value.Date;
            var end = f.CheckOut.Value.Date;

            q = q.Where(a => !_db.OrderItems
                .Include(oi => oi.Order)
                .Any(oi => oi.ApartmentId == a.Id
                    && oi.Order.Status == "Paid"
                    && !(oi.EndDate <= start || oi.StartDate >= end)   // overlap
                ));
        }

        // Order newest first
        var apartments = await q.OrderByDescending(a => a.Id).ToListAsync();

        // Populate distinct cities for the dropdown
        ViewBag.Cities = await _db.Apartments
            .Select(a => a.City)
            .Where(c => c != null && c != "")
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        ViewBag.Filter = f;
        return View(apartments);
    }



    public async Task<IActionResult> Detailes(int id)
        {
            var apt = await _db.Apartments
                .Include(a => a.Reviews)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (apt == null)
            {
                TempData["Msg"] = "That apartment wasn’t found.";
                return RedirectToAction(nameof(Index));
            }

            return View("Detailes", apt);
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
                existing.Quantity += 1; 
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