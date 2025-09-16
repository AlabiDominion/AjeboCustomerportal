namespace AjeboCustomerPortal.ViewModels
{
    public class ApartmentListings
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

        // Aggregate display (from entity)
        public double AverageRating { get; set; }
        public int RatingsCount { get; set; }

        // Computed
        public string ImageUrl =>
            string.IsNullOrWhiteSpace(ImageName)
                ? "https://merchants.shifts.com.ng/SharedImages/apartments/placeholder.jpg"
                : $"https://merchants.shifts.com.ng/SharedImages/apartments/{ImageName}";

        public IEnumerable<string> AmenityList =>
            (Amenities ?? "")
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim());
    }

    public class AmenityItem { public string value { get; set; } = default!; }
}
