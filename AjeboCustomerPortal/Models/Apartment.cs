namespace AjeboCustomerPortal.Models
{
    public class Apartment
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Address { get; set; } = default!;
        public string? Description { get; set; }
        public string City { get; set; } = default!;
        public string? Landmark { get; set; }
        public string? Roadside { get; set; }

        public decimal Price { get; set; }
        public decimal DefaultFare { get; set; }

        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }

        public string? Balcony { get; set; }
        public string? RemoteArea { get; set; }
        public string? CommonArea { get; set; }
        public string? UrbanArea { get; set; }

        public string? Amenities { get; set; }
        public string? ImageName { get; set; }
        public string? SupportImage1 { get; set; }
        public string? SupportImage2 { get; set; }
        public string? SupportImage3 { get; set; }
        public string? SupportImage4 { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        // Ratings aggregate fields (optional but useful)
        public double AverageRating { get; set; }
        public int RatingsCount { get; set; }

        // Navigation
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

    }
}
