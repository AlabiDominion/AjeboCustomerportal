namespace AjeboCustomerPortal.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int ApartmentId { get; set; }
        public Apartment Apartment { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public UserDetailes User { get; set; } = default!;
        public int OrderId { get; set; }
        public Order Order { get; set; } = default!;
        public int Rating { get; set; } // 1..5
        public string? Title { get; set; }
        public string? Body { get; set; }
        public bool IsApproved { get; set; } = true; // set false if you want moderation
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}