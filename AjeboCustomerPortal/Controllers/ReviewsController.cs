using AjeboCustomerPortal.Data;
using AjeboCustomerPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class ReviewsController : Controller
{
    private readonly ApplicationDbContext _db;
    public ReviewsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Create(int apartmentId, int orderId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        var eligible = await _db.Orders
            .Include(o => o.Items)
            .AnyAsync(o => o.Id == orderId
                        && o.UserId == userId
                        && o.Status == "Paid"
                        && o.Items.Any(i => i.ApartmentId == apartmentId
                                         && i.EndDate <= DateTime.UtcNow.Date));

        if (!eligible || await _db.Reviews.AnyAsync(r => r.OrderId == orderId && r.ApartmentId == apartmentId && r.UserId == userId))
            return Forbid();

        var vm = new ReviewVm { ApartmentId = apartmentId, OrderId = orderId, Rating = 5 };
        ViewBag.Apartment = await _db.Apartments.FindAsync(apartmentId);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReviewVm vm)
    {
        if (vm.Rating < 1 || vm.Rating > 5)
            ModelState.AddModelError(nameof(vm.Rating), "Rating must be between 1 and 5.");

        if (!ModelState.IsValid)
        {
            ViewBag.Apartment = await _db.Apartments.FindAsync(vm.ApartmentId);
            return View(vm);
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        // Re-validate eligibility + uniqueness
        var eligible = await _db.Orders
            .Include(o => o.Items)
            .AnyAsync(o => o.Id == vm.OrderId
                        && o.UserId == userId
                        && o.Status == "Paid"
                        && o.Items.Any(i => i.ApartmentId == vm.ApartmentId
                                         && i.EndDate <= DateTime.UtcNow.Date));

        if (!eligible || await _db.Reviews.AnyAsync(r => r.OrderId == vm.OrderId && r.ApartmentId == vm.ApartmentId && r.UserId == userId))
            return Forbid();

        var review = new Review
        {
            ApartmentId = vm.ApartmentId,
            OrderId = vm.OrderId,
            UserId = userId,
            Rating = vm.Rating,
            Title = vm.Title,
            Body = vm.Body,
            IsApproved = true
        };

        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();

        // Update aggregates
        await UpdateApartmentRating(vm.ApartmentId);

        var exists = await _db.Apartments.AnyAsync(a => a.Id == vm.ApartmentId);
        if (exists)
            return RedirectToAction("Details", "Apartments", new { id = vm.ApartmentId });

        TempData["Msg"] = "Review submitted. The apartment page is no longer available.";
        return RedirectToAction("My", "Orders");
    }

    private async Task UpdateApartmentRating(int apartmentId)
    {
        var revs = await _db.Reviews
            .Where(r => r.ApartmentId == apartmentId && r.IsApproved)
            .Select(r => r.Rating)
            .ToListAsync();

        var apt = await _db.Apartments.FindAsync(apartmentId);
        if (apt != null)
        {
            apt.RatingsCount = revs.Count;
            apt.AverageRating = revs.Count == 0 ? 0 : revs.Average();
            await _db.SaveChangesAsync();
        }
       

    }
}

public class ReviewVm
{
    public int ApartmentId { get; set; }
    public int OrderId { get; set; }
    public int Rating { get; set; } = 5; // 1..5
    public string? Title { get; set; }
    public string? Body { get; set; }
}
