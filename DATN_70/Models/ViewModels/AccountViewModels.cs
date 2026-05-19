using System.ComponentModel.DataAnnotations;

namespace DATN_70.Models.ViewModels;

public sealed class AccountProfileViewModel
{
    public string TaiKhoanId { get; set; } = string.Empty;
    public string KhachHangId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Địa chỉ mặc định")]
    public string DefaultAddressText { get; set; } = string.Empty;

    public string? StatusMessage { get; set; }
}

public sealed class AccountAddressPageViewModel
{
    public AddressFormViewModel Form { get; set; } = new();
    public List<AccountAddressItemViewModel> Addresses { get; set; } = [];
    public string? StatusMessage { get; set; }
}

public sealed class AddressFormViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên người nhận.")]
    [Display(Name = "Tên người nhận")]
    public string RecipientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số nhà, tên đường.")]
    [Display(Name = "Địa chỉ chi tiết")]
    public string Street { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tỉnh/thành phố.")]
    [Display(Name = "Tỉnh/Thành phố")]
    public string Province { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập quận/huyện.")]
    [Display(Name = "Quận/Huyện")]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập phường/xã.")]
    [Display(Name = "Phường/Xã")]
    public string Ward { get; set; } = string.Empty;

    [Display(Name = "Đặt làm mặc định")]
    public bool IsDefault { get; set; }

    public bool IsEdit => !string.IsNullOrWhiteSpace(Id);
}

public sealed class AccountAddressItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string FullAddress => string.Join(", ", new[] { Street, Ward, District, Province }.Where(part => !string.IsNullOrWhiteSpace(part)));
}

public sealed class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu hiện tại")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? StatusMessage { get; set; }
}

public sealed class AccountOrdersPageViewModel
{
    public string CurrentFilter { get; set; } = "all";
    public List<AccountOrderViewModel> Orders { get; set; } = [];
}

public sealed class AccountOrderViewModel
{
    public string Id { get; set; } = string.Empty;
    public string StatusKey { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public List<AccountOrderLineViewModel> Items { get; set; } = [];
}

public sealed class AccountOrderLineViewModel
{
<<<<<<< HEAD
    public string ChiTietSanPhamId { get; set; }
=======
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
    public string ProductName { get; set; } = string.Empty;
    public string Variant { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
<<<<<<< HEAD
public class CustomerReturnRequest
{
    public string HoaDonId { get; set; } = string.Empty;
    public string GhiChu { get; set; } = string.Empty;
    public List<CustomerReturnItem> Items { get; set; } = new();
}

public class CustomerReturnItem
{
    public string ChiTietSanPhamId { get; set; } = string.Empty;
    public int SoLuongTra { get; set; }
    public int LyDoKey { get; set; }
}
=======
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
