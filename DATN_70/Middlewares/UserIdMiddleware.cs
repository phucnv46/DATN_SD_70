using System.Security.Claims;

namespace DATN_70.Middlewares;

public class UserIdMiddleware
{
    private readonly RequestDelegate _next;

    public UserIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Giả định cậu đã có cơ chế xác thực JWT và lưu Claim "TaiKhoanID"
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var taiKhoanIdClaim = context.User.FindFirst("TaiKhoanID");
            if (taiKhoanIdClaim != null)
            {
                context.Items["UserId"] = taiKhoanIdClaim.Value;
            }
        }
        // Nếu chưa có JWT, ta vẫn có thể test bằng cách dùng Session
        else if (context.Session.GetString("TaiKhoanID") != null)
        {
            context.Items["UserId"] = context.Session.GetString("TaiKhoanID");
        }

        await _next(context);
    }
}