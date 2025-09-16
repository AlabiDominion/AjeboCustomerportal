using System;
using System.Threading.Tasks;
using AjeboCustomerPortal.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AjeboCustomerPortal.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public PaymentsController(ApplicationDbContext db) => _db = db;

        // GET: /Payments/Pay?orderId=123
        public async Task<IActionResult> Pay(int orderId)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null || order.Status != "Pending") return NotFound();

            if (string.IsNullOrWhiteSpace(order.PaymentRef))
            {
                order.PaymentRef = $"AJ-{orderId}-{Guid.NewGuid().ToString("N")[..8]}";
                await _db.SaveChangesAsync();
            }

            // Render a simple page that would normally initialize your payment gateway
            return View(order);
        }

        // GET: /Payments/Callback?reference=AJ-...
        [AllowAnonymous]
        public async Task<IActionResult> Callback(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference)) return BadRequest();
            var order = await _db.Orders.FirstOrDefaultAsync(o => o.PaymentRef == reference);
            if (order == null) return NotFound();

            // TODO: verify via gateway webhook or API; for now, mark paid for demo
            order.Status = "Paid";
            await _db.SaveChangesAsync();

            return RedirectToAction("Receipt", "Orders", new { orderId = order.Id });
        }

        // POST: /Payments/Webhook  (to be implemented with your gateway)
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Webhook()
        {
            // Validate signature, find order by ref, set Paid if amount matches.
            return Ok();
        }
    }
}