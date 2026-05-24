using Microsoft.AspNetCore.Mvc;

namespace DATN_70.Controllers.Admin;

public class ReportsController : Controller
{
    [Route("/Admin/Reports/Advanced")]
    public IActionResult Advanced()
    {
        return View("~/Views/Admin/Reports/Advanced.cshtml");
    }
}