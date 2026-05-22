using DATN_70.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace DATN_70.Controllers.Admin;
[CustomAuthorize("R01", "R02")]
public class AdminController : Controller
{
    
    // Bật giao diện Quản lý hóa đơn
    public IActionResult Orders()
    {
        return View();
    }

    // Bật giao diện Quản lý tài khoản
    public IActionResult Accounts()
    {
        return View();
    }

    // Bật giao diện Quản lý khuyến mãi
    public IActionResult Promotions()
    {
        return View();
    }

    // ĐIỀU HƯỚNG TRANG SẢN PHẨM
    public IActionResult Products()
    {
        return View();
    }
    // Thêm vào cuối file AdminController.cs của bạn
    public IActionResult Attributes()
    {
        return View();
    }
    [HttpGet]
    [CustomAuthorize("R01", "R02")]
    public IActionResult Dashboard()
    {
        return View();
    }
}