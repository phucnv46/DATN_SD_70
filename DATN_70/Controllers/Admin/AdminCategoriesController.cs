using DATN_70.Data;
using DATN_70.Models.Admin;
using DATN_70.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN_70.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/categories")]
    public class AdminCategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminCategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // 1. LẤY DANH SÁCH (READ)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _context.DanhMucs
                .Select(c => new { c.DanhMucID, c.Ten })
                .ToListAsync();

            return Ok(categories);
        }

        // 2. THÊM MỚI (CREATE)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryCreateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Sinh mã ID ngẫu nhiên bắt đầu bằng DM
            var newId = "DM" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

            var danhMuc = new DanhMuc
            {
                DanhMucID = newId,
                Ten = request.Ten
            };

            _context.DanhMucs.Add(danhMuc);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Thêm danh mục thành công", data = danhMuc });
        }

        // 3. CẬP NHẬT (UPDATE)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CategoryUpdateRequest request)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc == null) return NotFound(new { message = "Không tìm thấy danh mục" });

            danhMuc.Ten = request.Ten;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thành công", data = danhMuc });
        }

        // 4. XÓA (DELETE)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var danhMuc = await _context.DanhMucs.FindAsync(id);
            if (danhMuc == null) return NotFound(new { message = "Không tìm thấy danh mục" });

            // Kiểm tra xem danh mục có đang chứa sản phẩm nào không (chống xóa nhầm)
            var hasProducts = await _context.SanPhams.AnyAsync(sp => sp.DanhMucID == id);
            if (hasProducts)
            {
                return BadRequest(new { message = "Không thể xóa vì đang có sản phẩm thuộc danh mục này!" });
            }

            _context.DanhMucs.Remove(danhMuc);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa danh mục thành công" });
        }
    }
}