<<<<<<< HEAD
﻿using System.ComponentModel.DataAnnotations;
=======
using System.ComponentModel.DataAnnotations;
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4

namespace DATN_70.Models.Orders;

public sealed class PlaceOrderRequest
{
<<<<<<< HEAD
    [Required(ErrorMessage = "Mã khách hàng không hợp lệ.")]
    public string KhachHangID { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã địa chỉ không hợp lệ.")]
    public string DiaChiID { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên người nhận không được để trống.")]
=======
    [Required]
    [StringLength(100)]
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
    public string TenKhachHang { get; set; } = string.Empty;

    [Required]
    [StringLength(15)]
    public string SoDienThoai { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string DiaChiGiaoHang { get; set; } = string.Empty;

    [MinLength(1)]
    public List<OrderItemRequest> Items { get; set; } = [];
<<<<<<< HEAD
}
=======
}
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
