using System.Threading.Tasks;
using AjeboCustomerPortal.Data;
using AjeboCustomerPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AjeboCustomerPortal.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ReceiptPdfService _pdf;

        // Inject both DbContext and the PDF service
        public OrdersController(ApplicationDbContext db, ReceiptPdfService pdf)
        {
            _db = db;
            _pdf = pdf;
        }

        // GET: /Orders/Receipt/5
        public async Task<IActionResult> Receipt(int orderId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

            var order = await _db.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Apartment)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null) return NotFound();

            return View(order);
        }

        // GET: /Orders/ReceiptPdf/5
        public async Task<IActionResult> ReceiptPdf(int orderId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

            var order = await _db.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Apartment)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null) return NotFound();

            var pdfBytes = _pdf.Generate(order);
            var fileName = $"Ajebo_Receipt_{order.Id}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}
