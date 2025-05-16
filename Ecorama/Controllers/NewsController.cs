using Microsoft.AspNetCore.Mvc;

namespace Ecorama.Controllers
{
    public class NewsController : Controller
    {
        public IActionResult News()
        {
            return View();
        }

        public IActionResult Advertisements()
        {
            return View();
        }
        // it should get id 
        public IActionResult AdvertisementsDetails()
        {
            return View();
        }
        // it should get id 
        public IActionResult NewsDetails()
        {
            return View();
        }
    }
}

