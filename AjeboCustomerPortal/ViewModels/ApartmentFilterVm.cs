namespace AjeboCustomerPortal.ViewModels
{
    public class ApartmentFilterVm
    {
        public string? Location { get; set; }          
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public int? Guests { get; set; }               
        public decimal? MinBudget { get; set; }
        public decimal? MaxBudget { get; set; }
    }
}
