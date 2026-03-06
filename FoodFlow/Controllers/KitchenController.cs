using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodFlow.Controllers
{
    [Authorize(Roles = "Cook")]
    public class KitchenController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
