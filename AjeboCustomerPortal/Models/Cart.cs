namespace AjeboCustomerPortal.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; } = default!;
        public UserDetailes User { get; set; } = default!;
        public bool IsCheckedOut { get; set; }
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }

    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public Cart Cart { get; set; } = default!;
        public int ApartmentId { get; set; }
        public Apartment Apartment { get; set; } = default!;
        public int Quantity { get; set; } = 1;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}