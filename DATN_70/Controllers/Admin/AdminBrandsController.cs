using DATN_70.Data;
using DATN_70.Models.Admin;
using DATN_70.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN_70.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/brands")]
    public class AdminBrandsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env; // Thêm biến môi trường để lưu file

        public AdminBrandsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var brands = await _context.ThuongHieus.ToListAsync();
            return Ok(brands);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] BrandCreateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Xử lý lưu File ảnh
            string logoUrl = "";
            if (request.LogoFile != null && request.LogoFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString("N").Substring(0, 8) + Path.GetExtension(request.LogoFile.FileName);
                var path = Path.Combine(_env.WebRootPath, "images", "brands", fileName);

                // Tạo thư mục nếu chưa tồn tại
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory!);

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await request.LogoFile.CopyToAsync(fileStream);
                }
                logoUrl = "/images/brands/" + fileName;
            }

            var newId = "TH" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

            var thuongHieu = new ThuongHieu
            {
                ThuongHieuID = newId,
                Ten = request.Ten,
                MoTa = request.MoTa ?? "",
                LogoURL = logoUrl // Gán đường dẫn file vừa lưu
            };

            _context.ThuongHieus.Add(thuongHieu);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm thương hiệu thành công", data = thuongHieu });
        }

        // 3. CẬP NHẬT THƯƠNG HIỆU (Chuyển sang [FromForm])
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromForm] BrandUpdateRequest request)
        {
            var thuongHieu = await _context.ThuongHieus.FindAsync(id);
            if (thuongHieu == null) return NotFound(new { message = "Không tìm thấy thương hiệu" });

            thuongHieu.Ten = request.Ten;
            thuongHieu.MoTa = request.MoTa ?? "";

            // Nếu người dùng chọn file ảnh mới thì mới xử lý thay đổi ảnh
            if (request.LogoFile != null && request.LogoFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString("N").Substring(0, 8) + Path.GetExtension(request.LogoFile.FileName);
                var path = Path.Combine(_env.WebRootPath, "images", "brands", fileName);

                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory!);

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await request.LogoFile.CopyToAsync(fileStream);
                }

                // Cập nhật URL mới (Bạn có thể bổ sung code xóa file ảnh cũ ở đây nếu muốn sạch host)
                thuongHieu.LogoURL = "/images/brands/" + fileName;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công", data = thuongHieu });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var thuongHieu = await _context.ThuongHieus.FindAsync(id);
            if (thuongHieu == null) return NotFound(new { message = "Không tìm thấy thương hiệu" });

            var hasProducts = await _context.SanPhams.AnyAsync(sp => sp.ThuongHieuID == id);
            if (hasProducts)
            {
                return BadRequest(new { message = "Không thể xóa vì đang có sản phẩm thuộc thương hiệu này!" });
            }

            _context.ThuongHieus.Remove(thuongHieu);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa thương hiệu thành công" });
        }
    }
}