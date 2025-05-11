using Microsoft.AspNetCore.Mvc;

namespace Ecorama.Controllers
{
    public class WorkShopsController : Controller
    {
        public IActionResult Workshops()
        {
            return View();
        }
    }
}
