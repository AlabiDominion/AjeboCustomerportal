using System.Linq;
using AjeboCustomerPortal.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AjeboCustomerPortal.Services
{
    public class ReceiptPdfService
    {
        public byte[] Generate(Order order)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36);
                    page.DefaultTextStyle(x => x.FontSize(11));
                    page.Content().Column(col =>
                    {
                        // Header
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Ajebo Apartment Receipt").SemiBold().FontSize(16);
                            r.ConstantItem(200).AlignRight().Column(rc =>
                            {
                                rc.Item().Text($"Order #: {order.Id}");
                                rc.Item().Text($"Payment Ref: {order.PaymentRef ?? "-"}");
                                rc.Item().Text($"Status: {order.Status}");
                                rc.Item().Text($"Date: {order.CreatedAt:yyyy-MM-dd HH:mm}");
                            });
                        });

                        col.Item().PaddingVertical(8).LineHorizontal(0.5f);

                        // Items table
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3); // Apartment
                                c.RelativeColumn(3); // Dates
                                c.RelativeColumn(1); // Nights
                                c.RelativeColumn(1); // Unit
                                c.RelativeColumn(1); // Qty
                                c.RelativeColumn(1); // Line
                            });

                            // Header row
                            t.Header(h =>
                            {
                                h.Cell().Element(CellHeader).Text("Apartment");
                                h.Cell().Element(CellHeader).Text("Dates");
                                h.Cell().Element(CellHeader).AlignRight().Text("Nights");
                                h.Cell().Element(CellHeader).AlignRight().Text("Unit");
                                h.Cell().Element(CellHeader).AlignRight().Text("Qty");
                                h.Cell().Element(CellHeader).AlignRight().Text("Line total");
                                static IContainer CellHeader(IContainer c) => c.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(4).BorderBottom(0.5f);
                            });

                            foreach (var i in order.Items)
                            {
                                var nights = (i.EndDate.Date - i.StartDate.Date).Days;
                                t.Cell().Text(i.Apartment?.Name ?? $"Apartment #{i.ApartmentId}");
                                t.Cell().Text($"{i.StartDate:yyyy-MM-dd} → {i.EndDate:yyyy-MM-dd}");
                                t.Cell().AlignRight().Text(nights.ToString());
                                t.Cell().AlignRight().Text($"₦{i.UnitPrice:N2}");
                                t.Cell().AlignRight().Text(i.Quantity.ToString());
                                t.Cell().AlignRight().Text($"₦{(i.UnitPrice * nights * i.Quantity):N2}");
                            }
                        });

                        col.Item().PaddingVertical(8).LineHorizontal(0.5f);

                        // Totals
                        col.Item().AlignRight().Text($"Total: ₦{order.TotalAmount:N2}").SemiBold().FontSize(13);

                        // Footer / thanks
                        col.Item().PaddingTop(16).Text("Thank you for your booking!").FontSize(11);
                    });
                });
            }).GeneratePdf();
        }
    }
}
