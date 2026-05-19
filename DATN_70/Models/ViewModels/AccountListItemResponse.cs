namespace DATN_70.Models.ViewModels
{
    public class AccountListItemResponse
    {
        public string TaiKhoanID { get; set; }
        public string Email { get; set; }
        public string TenVaiTro { get; set; }

        // Gộp chung tên của Khách hàng hoặc Nhân viên vào đây để hiển thị ra 1 cột
        public string TenChuTaiKhoan { get; set; }
        public string SoDienThoai { get; set; }

        // Vì TrangThai của nhóm đang là string
        public string TrangThai { get; set; }
    }
}