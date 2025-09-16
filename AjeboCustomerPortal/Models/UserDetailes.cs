using Microsoft.AspNetCore.Identity;
namespace AjeboCustomerPortal.Models
{
    public class UserDetailes : IdentityUser
    {
        public string FullName { get; set; } = default!;

    }
}