using AjeboCustomerPortal.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    public HomeController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var apartments = await _db.Apartments
                                  .OrderByDescending(a => a.Id)
                                  .ToListAsync();
        return View(apartments);
    }
}
