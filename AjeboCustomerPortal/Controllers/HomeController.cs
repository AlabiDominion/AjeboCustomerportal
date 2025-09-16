using Microsoft.AspNetCore.Mvc;

namespace AjeboCustomerPortal.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
