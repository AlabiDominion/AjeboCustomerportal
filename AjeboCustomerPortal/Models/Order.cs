namespace AjeboCustomerPortal.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = default!;
        public UserDetailes User { get; set; } = default!;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public decimal TotalAmount { get; set; }
        public string? PaymentRef { get; set; }
        public string Status { get; set; } = "Pending"; // Pending|Paid|Failed|Cancelled
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = default!;
        public int ApartmentId { get; set; }
        public Apartment Apartment { get; set; } = default!;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal Subtotal => UnitPrice * Quantity;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}