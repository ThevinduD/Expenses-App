using Microsoft.AspNetCore.Mvc;

namespace MIS_DEMO.Controllers
{
    public class SalesController : Controller
    {
        public IActionResult Details()
        {
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
    }
}
