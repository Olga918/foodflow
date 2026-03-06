using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodFlow.Controllers
{
    [Authorize(Roles = "Storekeeper")]
    public class StockController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
