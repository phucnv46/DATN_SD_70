<<<<<<< HEAD
﻿namespace DATN_70.Models.ViewModels;
=======
namespace DATN_70.Models.ViewModels;
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4

public sealed class HomeIndexViewModel
{
    public List<HomeBannerViewModel> Banners { get; set; } = [];
<<<<<<< HEAD
    public List<HomeProductViewModel> FeaturedProducts { get; set; } = [];
    public List<HomeProductViewModel> SaleProducts { get; set; } = [];
=======
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
}

public sealed class HomeBannerViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string LinkUrl { get; set; } = string.Empty;
}

public sealed class BannerManagementViewModel
{
    public List<BannerListItemViewModel> Items { get; set; } = [];
}

public sealed class BannerListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string LinkUrl { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
<<<<<<< HEAD
public sealed class HomeProductViewModel
{
    public string SanPhamID { get; set; } = string.Empty;
    public string TenSanPham { get; set; } = string.Empty;
    public string DanhMuc { get; set; } = string.Empty; // Chứa tên thương hiệu hoặc nhóm
    public decimal GiaThapNhat { get; set; }
    public decimal GiaGoc { get; set; }
    public double PhanTramGiam { get; set; }
    public string HinhAnhUrl { get; set; } = string.Empty;
    public string MoTaNgan { get; set; } = string.Empty;
}
=======
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
