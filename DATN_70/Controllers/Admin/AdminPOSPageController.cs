using DATN_70.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace DATN_70.Controllers.Admin;
[CustomAuthorize("R01", "R02")]
public class AdminPOSPageController : Controller
{

    // Đường dẫn truy cập trực tiếp vào không gian quầy thu ngân: /Admin/POS
    [HttpGet("/Admin/POS")]
    public IActionResult Index()
    {
        // Điều hướng chuẩn xác tới vị trí file Pos.cshtml bạn vừa tạo
        return View("~/Views/Admin/Pos.cshtml");
    }
}