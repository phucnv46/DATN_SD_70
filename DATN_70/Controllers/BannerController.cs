using DATN_70.Data;
using DATN_70.Models.Entities;
using DATN_70.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN_70.Controllers;

public class BannerController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public BannerController(AppDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new BannerManagementViewModel
        {
            Items = await _dbContext.Banners
                .AsNoTracking()
                .OrderBy(item => item.ThuTu)
                .ThenBy(item => item.TieuDe)
                .Select(item => new BannerListItemViewModel
                {
                    Id = item.BannerID,
                    Title = item.TieuDe,
                    Description = item.MoTa ?? string.Empty,
                    ImageUrl = item.HinhAnhUrl,
                    LinkUrl = item.LienKetUrl ?? string.Empty,
                    SortOrder = item.ThuTu,
                    IsActive = item.KichHoat
                })
                .ToListAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string title,
        string? description,
        string? linkUrl,
        int sortOrder,
        bool isActive,
        IFormFile? imageFile)
    {
        if (imageFile is null || imageFile.Length == 0)
        {
            TempData["BannerMessage"] = "Vui long chon anh banner.";
            return RedirectToAction(nameof(Index));
        }

        var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "banners");
        Directory.CreateDirectory(uploadsRoot);

        var extension = Path.GetExtension(imageFile.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(uploadsRoot, fileName);

        await using (var stream = System.IO.File.Create(absolutePath))
        {
            await imageFile.CopyToAsync(stream);
        }

        _dbContext.Banners.Add(new Banner
        {
            BannerID = Guid.NewGuid().ToString(),
            TieuDe = string.IsNullOrWhiteSpace(title) ? "Banner moi" : title.Trim(),
            MoTa = description?.Trim(),
            HinhAnhUrl = $"/uploads/banners/{fileName}",
            LienKetUrl = linkUrl?.Trim(),
            ThuTu = sortOrder,
            KichHoat = isActive
        });

        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(string id)
    {
        var banner = await _dbContext.Banners.FirstOrDefaultAsync(item => item.BannerID == id);
        if (banner is null)
        {
            return RedirectToAction(nameof(Index));
        }

        banner.KichHoat = !banner.KichHoat;
        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var banner = await _dbContext.Banners.FirstOrDefaultAsync(item => item.BannerID == id);
        if (banner is null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (!string.IsNullOrWhiteSpace(banner.HinhAnhUrl) && banner.HinhAnhUrl.StartsWith("/uploads/banners/", StringComparison.OrdinalIgnoreCase))
        {
            var physicalPath = Path.Combine(_environment.WebRootPath, banner.HinhAnhUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
        }

        _dbContext.Banners.Remove(banner);
        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
