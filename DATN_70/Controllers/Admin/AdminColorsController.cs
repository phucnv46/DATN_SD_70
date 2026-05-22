using DATN_70.Attributes;
using DATN_70.Data;
using DATN_70.Models.Admin;
using DATN_70.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN_70.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/colors")]
    [CustomAuthorize("R01", "R02")]
    public class AdminColorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminColorsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. LẤY DANH SÁCH MÀU
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var colors = await _context.Maus.ToListAsync();
            return Ok(colors);
        }

        // 2. THÊM MỚI MÀU
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ColorCreateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newId = "M" + Guid.NewGuid().ToString("N").Substring(0, 7).ToUpper();

            var mau = new Mau
            {
                MauID = newId,
                Ten = request.Ten
            };

            _context.Maus.Add(mau);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm màu sắc thành công", data = mau });
        }

        // 3. CẬP NHẬT MÀU
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] ColorUpdateRequest request)
        {
            var mau = await _context.Maus.FindAsync(id);
            if (mau == null) return NotFound(new { message = "Không tìm thấy màu sắc" });

            mau.Ten = request.Ten;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thành công", data = mau });
        }

        // 4. XÓA MÀU
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var mau = await _context.Maus.FindAsync(id);
            if (mau == null) return NotFound(new { message = "Không tìm thấy màu sắc" });

            // Kiểm tra xem có Chi tiết sản phẩm (Biến thể) nào đang dùng màu này không
            var hasVariants = await _context.ChiTietSanPhams.AnyAsync(ct => ct.MauID == id);
            if (hasVariants)
            {
                return BadRequest(new { message = "Không thể xóa vì đang có sản phẩm sử dụng màu này!" });
            }

            _context.Maus.Remove(mau);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa màu sắc thành công" });
        }
    }
}