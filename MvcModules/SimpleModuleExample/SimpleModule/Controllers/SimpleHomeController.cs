using System.Web.Mvc;

namespace SimpleModule.Controllers
{
    public class SimpleHomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Your module description page.";
            return View();
        }
    }
}
