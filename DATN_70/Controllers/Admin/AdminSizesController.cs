using DATN_70.Attributes;
using DATN_70.Data;
using DATN_70.Models.Admin;
using DATN_70.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN_70.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/sizes")]
    [CustomAuthorize("R01", "R02")]
    public class AdminSizesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminSizesController(AppDbContext context)
        {
            _context = context;
        }

        // 1. LẤY DANH SÁCH KÍCH CỠ (GET ALL)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sizes = await _context.KichCos.ToListAsync();
            return Ok(sizes);
        }

        // 2. THÊM MỚI KÍCH CỠ (ĐÃ THÊM BẪY LỖI ĐỂ TRUY TÌM NGUYÊN NHÂN)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SizeCreateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // Sinh mã ID bắt đầu bằng chữ S (Size)
                var newId = "S" + Guid.NewGuid().ToString("N").Substring(0, 7).ToUpper();

                var kichCo = new KichCo
                {
                    KichCoID = newId,
                    Ten = request.Ten,
                    MoTa = ""
                };

                _context.KichCos.Add(kichCo);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm kích cỡ thành công", data = kichCo });
            }
            catch (Exception ex)
            {
                // ÉP SERVER PHẢI KHẠC LỖI CHI TIẾT RA TRÌNH DUYỆT
                return StatusCode(500, new
                {
                    message = "Hệ thống SQL Server hoặc EF Core bị sập ngầm!",
                    error = ex.Message, // Lỗi vòng ngoài
                    innerError = ex.InnerException?.Message // Lỗi gốc bên dưới SQL
                });
            }
        }

        // 3. CẬP NHẬT KÍCH CỠ (PUT)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] SizeUpdateRequest request)
        {
            var kichCo = await _context.KichCos.FindAsync(id);
            if (kichCo == null) return NotFound(new { message = "Không tìm thấy kích cỡ" });

            kichCo.Ten = request.Ten;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thành công", data = kichCo });
        }

        // 4. XÓA KÍCH CỠ (DELETE)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var kichCo = await _context.KichCos.FindAsync(id);
            if (kichCo == null) return NotFound(new { message = "Không tìm thấy kích cỡ" });

            // Kiểm tra xem có Biến thể sản phẩm (ChiTietSanPham) nào đang dùng size này không
            var hasVariants = await _context.ChiTietSanPhams.AnyAsync(ct => ct.KichCoID == id);
            if (hasVariants)
            {
                return BadRequest(new { message = "Không thể xóa vì đang có sản phẩm sử dụng kích cỡ này!" });
            }

            _context.KichCos.Remove(kichCo);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa kích cỡ thành công" });
        }
    }
}