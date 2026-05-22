// Đặt trong namespace DATN_70.Controllers hoặc Models.Orders
using System.ComponentModel.DataAnnotations;

public class CreateOnlinePaymentRequest
{
    [Required(ErrorMessage = "Mã hóa đơn không được để trống")]
    public string HoaDonID { get; set; } = string.Empty;
}