using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DATN_70.Attributes;

public class CustomAuthorizeAttribute : ActionFilterAttribute
{
    private readonly string[] _allowedRoles;

    // Truyền danh sách các mã VaiTroID được phép truy cập (Ví dụ: "R01", "R02")
    public CustomAuthorizeAttribute(params string[] roles)
    {
        _allowedRoles = roles;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        var userId = session.GetString("UserId");

        // KHÔNG GIAN BẢO MẬT 1: Chưa đăng nhập -> Đá bay lập tức về trang Login gốc
        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        // KHÔNG GIAN BẢO MẬT 2: Đã đăng nhập nhưng kiểm tra quyền hạn vai trò
        if (_allowedRoles != null && _allowedRoles.Length > 0)
        {
            var userRole = session.GetString("UserRole"); // Bốc quyền từ Session ra đối chiếu

            if (string.IsNullOrEmpty(userRole) || !_allowedRoles.Contains(userRole))
            {
                // Không khớp vai trò cho phép -> Điều hướng sang trang báo lỗi 403 Access Denied
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }
        }

        base.OnActionExecuting(context);
    }
}