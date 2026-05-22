namespace DATN_70.Models.Enums
{
    public class Enums
    {
        // 1. Trạng thái Hóa Đơn
        public enum TrangThaiHoaDon
        {
            ChoDuyet = 0,
            DaXacNhan = 1,
            DangChuanBi = 2,
            DangGiao = 3,
            HoanThanh = 4,
            DaHuy = 5,
            DaDoiTra = 6,
            DangChoThanhToanQR = 7
        }
        // 2. Loại giao dịch
        public enum LoaiGiaoDich
        {
            Online = 0,
            PosTaiQuay = 1
        }
        // 3. Giới tính chung cho Khách Hàng & Nhân Viên
        public enum GioiTinh
        {
            Nam = 0,
            Nu = 1,
            Khac = 2
        }
        // 4. Trạng thái hoạt động
        public enum TrangThaiHoatDong
        {
            NgungHoatDong = 0,
            HoatDong = 1
        }
        //5. Kiểu thanh toán
        public enum KieuThanhToan
        {
            Online = 0,
            Offline = 1
        }
        //6. Trạng thái thanh toán
        public enum TrangThaiThanhToan
        {
            ThatBai = 0,
            ThanhCong = 1
        }
        public enum LoaiGiamGia
        {
            TruThangTien = 0,
            PhanTram = 1
        }
        // 7. Trạng thái Phiếu Đổi Trả
        public enum TrangThaiDoiTra
        {
            ChoXuLy = 0,
            DaHoanTien_NhapKho = 1, // Trạng thái này sẽ kích hoạt code cộng lại số lượng tồn
            TuChoi = 2
        }

        // 8. Lý do Đổi/Trả hàng
        public enum LyDoDoiTra
        {
            LoiNhaSanXuat = 0,
            SaiKichCoMauSac = 1,
            GiaoThieuHang = 2,
            Khac = 3
        }
    }
}
