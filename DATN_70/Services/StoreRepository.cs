using DATN_70.Data;
using DATN_70.Models.Orders;
using DATN_70.Models.Products;
using Microsoft.Data.SqlClient;

namespace DATN_70.Services;

public sealed class StoreRepository : IStoreRepository
{
    private readonly SqlConnectionFactory _connectionFactory;

    public StoreRepository(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ProductListItemResponse>> GetProductsAsync(CancellationToken cancellationToken)
    {
        // SQL đã được nâng cấp để đọc LoaiGiamGia, GiaTriGiam và GiamToiDa
        const string sql = """
            WITH ActivePromotions AS (
                SELECT
                    ksp.SanPhamID,
                    km.LoaiGiamGia,
                    km.GiaTriGiam,
                    km.GiamToiDa,
                    ROW_NUMBER() OVER(PARTITION BY ksp.SanPhamID ORDER BY km.GiaTriGiam DESC) as rn
                FROM KhuyenMaiSanPhams ksp
                INNER JOIN KhuyenMais km ON km.KhuyenMaiID = ksp.KhuyenMaiID
                WHERE km.TrangThai = 1
                  AND GETDATE() >= km.NgayApDung
                  AND GETDATE() <= km.NgayKetThuc
                  AND (km.SoLuongToiDa = 0 OR km.SoLuongDaDung < km.SoLuongToiDa)
            )
            SELECT
                sp.SanPhamID,
                sp.Ten,
                sp.MoTa,
                MIN(CASE
                    WHEN ap.GiaTriGiam IS NULL THEN ctsp.GiaNiemYet
                    WHEN ap.LoaiGiamGia = 0 THEN 
                         CASE WHEN ctsp.GiaNiemYet - ap.GiaTriGiam < 0 THEN 0 ELSE ctsp.GiaNiemYet - ap.GiaTriGiam END
                    WHEN ap.LoaiGiamGia = 1 THEN
                         CASE WHEN ap.GiamToiDa > 0 AND (ctsp.GiaNiemYet * ap.GiaTriGiam / 100.0) > ap.GiamToiDa
                              THEN ctsp.GiaNiemYet - ap.GiamToiDa
                              ELSE ctsp.GiaNiemYet - ROUND(ctsp.GiaNiemYet * ap.GiaTriGiam / 100.0, 0)
                         END
                    ELSE ctsp.GiaNiemYet
                END) AS GiaThapNhat,
                MIN(ctsp.GiaNiemYet) AS GiaGoc,
                COALESCE(
                    CASE WHEN ap.LoaiGiamGia = 1 THEN ap.GiaTriGiam
                         WHEN ap.LoaiGiamGia = 0 AND MIN(ctsp.GiaNiemYet) > 0 THEN (ap.GiaTriGiam / MIN(ctsp.GiaNiemYet)) * 100.0
                         ELSE 0 END, 0
                ) AS PhanTramGiam,
                COALESCE(ap.LoaiGiamGia, -1) AS LoaiGiamGia,
                COALESCE(ap.GiaTriGiam, 0) AS GiaTriGiam,
                COALESCE(ap.GiamToiDa, 0) AS GiamToiDa,
                SUM(ctsp.SoLuongTonKho) AS TongSoLuongTon
            FROM SanPhams sp
            LEFT JOIN ChiTietSanPhams ctsp ON ctsp.SanPhamID = sp.SanPhamID
            LEFT JOIN ActivePromotions ap ON ap.SanPhamID = sp.SanPhamID AND ap.rn = 1
            GROUP BY sp.SanPhamID, sp.Ten, sp.MoTa, ap.LoaiGiamGia, ap.GiaTriGiam, ap.GiamToiDa
            ORDER BY sp.Ten;
            """;

        var products = new List<ProductListItemResponse>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(new ProductListItemResponse
            {
                SanPhamID = reader.GetString(0),
                Ten = reader.GetString(1),
                MoTa = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                GiaThapNhat = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                GiaGoc = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4),
                PhanTramGiam = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                LoaiGiamGia = reader.GetInt32(6),
                GiaTriGiam = reader.GetDecimal(7),
                GiamToiDa = reader.GetDecimal(8),
                TongSoLuongTon = reader.IsDBNull(9) ? 0 : reader.GetInt32(9)
            });
        }

        return products;
    }

    public async Task<ProductDetailResponse?> GetProductDetailAsync(string productId, CancellationToken cancellationToken)
    {
        const string productSql = """
            SELECT SanPhamID, Ten, MoTa
            FROM SanPhams
            WHERE SanPhamID = @SanPhamID;
            """;

        const string variantSql = """
            WITH ActivePromotions AS (
                SELECT
                    ksp.SanPhamID,
                    km.LoaiGiamGia,
                    km.GiaTriGiam,
                    km.GiamToiDa,
                    ROW_NUMBER() OVER(PARTITION BY ksp.SanPhamID ORDER BY km.GiaTriGiam DESC) as rn
                FROM KhuyenMaiSanPhams ksp
                INNER JOIN KhuyenMais km ON km.KhuyenMaiID = ksp.KhuyenMaiID
                WHERE km.TrangThai = 1
                  AND GETDATE() >= km.NgayApDung
                  AND GETDATE() <= km.NgayKetThuc
                  AND (km.SoLuongToiDa = 0 OR km.SoLuongDaDung < km.SoLuongToiDa)
            )
            SELECT
                ctsp.ChiTietSanPhamID,
                ctsp.KichCoID,
                kc.Ten,
                ctsp.MauID,
                m.Ten,
                CASE
                    WHEN ap.GiaTriGiam IS NULL THEN ctsp.GiaNiemYet
                    WHEN ap.LoaiGiamGia = 0 THEN 
                         CASE WHEN ctsp.GiaNiemYet - ap.GiaTriGiam < 0 THEN 0 ELSE ctsp.GiaNiemYet - ap.GiaTriGiam END
                    WHEN ap.LoaiGiamGia = 1 THEN
                         CASE WHEN ap.GiamToiDa > 0 AND (ctsp.GiaNiemYet * ap.GiaTriGiam / 100.0) > ap.GiamToiDa
                              THEN ctsp.GiaNiemYet - ap.GiamToiDa
                              ELSE ctsp.GiaNiemYet - ROUND(ctsp.GiaNiemYet * ap.GiaTriGiam / 100.0, 0)
                         END
                    ELSE ctsp.GiaNiemYet
                END AS GiaBan,
                ctsp.GiaNiemYet AS GiaGoc,
                COALESCE(
                    CASE WHEN ap.LoaiGiamGia = 1 THEN ap.GiaTriGiam
                         WHEN ap.LoaiGiamGia = 0 AND ctsp.GiaNiemYet > 0 THEN (ap.GiaTriGiam / ctsp.GiaNiemYet) * 100.0
                         ELSE 0 END, 0
                ) AS PhanTramGiam,
                COALESCE(ap.LoaiGiamGia, -1) AS LoaiGiamGia,
                COALESCE(ap.GiaTriGiam, 0) AS GiaTriGiam,
                COALESCE(ap.GiamToiDa, 0) AS GiamToiDa,
                ctsp.SoLuongTonKho
            FROM ChiTietSanPhams ctsp
            INNER JOIN KichCos kc ON kc.KichCoID = ctsp.KichCoID
            INNER JOIN Maus m ON m.MauID = ctsp.MauID
            LEFT JOIN ActivePromotions ap ON ap.SanPhamID = ctsp.SanPhamID AND ap.rn = 1
            WHERE ctsp.SanPhamID = @SanPhamID
            ORDER BY kc.Ten, m.Ten;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var product = await LoadProductAsync(connection, productSql, productId, cancellationToken);
        if (product is null) return null;

        await using var variantCommand = new SqlCommand(variantSql, connection);
        variantCommand.Parameters.AddWithValue("@SanPhamID", productId);
        await using var reader = await variantCommand.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            product.BienThe.Add(new ProductVariantResponse
            {
                ChiTietSanPhamID = reader.GetString(0),
                KichCoID = reader.GetString(1),
                TenKichCo = reader.GetString(2),
                MauID = reader.GetString(3),
                TenMau = reader.GetString(4),
                GiaNiemYet = reader.GetDecimal(5),
                GiaGoc = reader.GetDecimal(6),
                PhanTramGiam = reader.GetDecimal(7),
                LoaiGiamGia = reader.GetInt32(8),
                GiaTriGiam = reader.GetDecimal(9),
                GiamToiDa = reader.GetDecimal(10),
                SoLuongTon = reader.GetInt32(11)
            });
        }

        return product;
    }

    public async Task<ServiceResult<OrderCreatedResponse>> PlaceOrderAsync(
        PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        var normalizedItems = request.Items.GroupBy(item => item.ChiTietSanPhamID).Select(group => new OrderItemRequest { ChiTietSanPhamID = group.Key, SoLuong = group.Sum(item => item.SoLuong) }).ToList();
        if (normalizedItems.Count == 0) return ServiceResult<OrderCreatedResponse>.Fail("Don hang phai co it nhat 1 san pham.");

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var orderId = GenerateId("HD", 20);
            var orderCreatedAt = DateTime.Now;
            decimal totalAmount = 0;
            var orderDetails = new List<(string DetailId, string ProductDetailId, int Quantity, decimal UnitPrice, decimal LineTotal)>();

            foreach (var item in normalizedItems)
            {
                var stockInfo = await GetStockInfoAsync(connection, transaction, item.ChiTietSanPhamID, cancellationToken);
                if (stockInfo is null) { await transaction.RollbackAsync(cancellationToken); return ServiceResult<OrderCreatedResponse>.Fail($"Khong tim thay bien the {item.ChiTietSanPhamID}."); }

                var stock = stockInfo.Value;
                if (stock.SoLuongTon < item.SoLuong) { await transaction.RollbackAsync(cancellationToken); return ServiceResult<OrderCreatedResponse>.Fail($"San pham {item.ChiTietSanPhamID} chi con {stock.SoLuongTon} trong kho."); }

                var detailId = GenerateId("HDCT", 20);
                var lineTotal = stock.GiaBan * item.SoLuong; // FIXED: Tính theo giá bán đã giảm thay vì giá gốc
                totalAmount += lineTotal;

                orderDetails.Add((detailId, item.ChiTietSanPhamID, item.SoLuong, stock.GiaBan, lineTotal));
            }

            await InsertOrderAsync(connection, transaction, orderId, request, orderCreatedAt, totalAmount, cancellationToken);

            foreach (var detail in orderDetails)
            {
                await InsertOrderDetailAsync(connection, transaction, orderId, detail, cancellationToken);
                
            }

            await transaction.CommitAsync(cancellationToken);
            return ServiceResult<OrderCreatedResponse>.Ok(new OrderCreatedResponse { HoaDonID = orderId, TenKhachHang = request.TenKhachHang, SoDienThoai = request.SoDienThoai, DiaChiGiaoHang = request.DiaChiGiaoHang, NgayTao = orderCreatedAt, TongTien = totalAmount, TrangThai = 0, ChiTiet = orderDetails.Select(detail => new OrderCreatedItemResponse { HoaDonChiTietID = detail.DetailId, ChiTietSanPhamID = detail.ProductDetailId, SoLuong = detail.Quantity, DonGia = detail.UnitPrice, ThanhTien = detail.LineTotal }).ToList() });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            Console.WriteLine("============= LỖI ĐẶT HÀNG TẠI ĐÂY: =============");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine("=================================================");
            throw;
        }
    }

    private static string GenerateId(string prefix, int totalLength)
    {
        var suffixLength = totalLength - prefix.Length;
        return prefix + Guid.NewGuid().ToString("N")[..suffixLength];
    }

    private static async Task<ProductDetailResponse?> LoadProductAsync(SqlConnection connection, string sql, string productId, CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@SanPhamID", productId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return new ProductDetailResponse { SanPhamID = reader.GetString(0), Ten = reader.GetString(1), MoTa = reader.IsDBNull(2) ? string.Empty : reader.GetString(2) };
    }

    private static async Task<(decimal GiaBan, int SoLuongTon)?> GetStockInfoAsync(SqlConnection connection, SqlTransaction transaction, string productDetailId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                CASE
                    WHEN ap.GiaTriGiam IS NULL THEN ctsp.GiaNiemYet
                    WHEN ap.LoaiGiamGia = 0 THEN 
                         CASE WHEN ctsp.GiaNiemYet - ap.GiaTriGiam < 0 THEN 0 ELSE ctsp.GiaNiemYet - ap.GiaTriGiam END
                    WHEN ap.LoaiGiamGia = 1 THEN
                         CASE WHEN ap.GiamToiDa > 0 AND (ctsp.GiaNiemYet * ap.GiaTriGiam / 100.0) > ap.GiamToiDa
                              THEN ctsp.GiaNiemYet - ap.GiamToiDa
                              ELSE ctsp.GiaNiemYet - ROUND(ctsp.GiaNiemYet * ap.GiaTriGiam / 100.0, 0)
                         END
                    ELSE ctsp.GiaNiemYet
                END AS GiaBan,
                ctsp.SoLuongTonKho
            FROM ChiTietSanPhams ctsp WITH (UPDLOCK, ROWLOCK)
            OUTER APPLY (
                SELECT TOP 1
                    km.LoaiGiamGia,
                    km.GiaTriGiam,
                    km.GiamToiDa
                FROM KhuyenMaiSanPhams ksp
                INNER JOIN KhuyenMais km ON km.KhuyenMaiID = ksp.KhuyenMaiID
                WHERE ksp.SanPhamID = ctsp.SanPhamID
                  AND km.TrangThai = 1
                  AND GETDATE() >= km.NgayApDung
                  AND GETDATE() <= km.NgayKetThuc
                  AND (km.SoLuongToiDa = 0 OR km.SoLuongDaDung < km.SoLuongToiDa)
                ORDER BY km.GiaTriGiam DESC
            ) ap
            WHERE ctsp.ChiTietSanPhamID = @ChiTietSanPhamID;
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@ChiTietSanPhamID", productDetailId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return (reader.GetDecimal(0), reader.GetInt32(1));
    }

    private static async Task InsertOrderAsync(SqlConnection connection, SqlTransaction transaction, string orderId, PlaceOrderRequest request, DateTime createdAt, decimal totalAmount, CancellationToken cancellationToken)
    {
        const string sql = @"
INSERT INTO HoaDons (HoaDonID, TongTienVAT, TongTienGiamGia, ThanhTien, LoaiGiaoDich, NgayTao, TrangThai, GhiChu, KhachHangID, NhanVienID, DiaChiID, KhuyenMaiID)
VALUES (@HoaDonID, @TongTienVAT, 0, @ThanhTien, 0, @NgayTao, @TrangThai, @GhiChu, @KhachHangID, N'NV0001', @DiaChiID, NULL)";
        decimal tienVat = Math.Round(totalAmount * 0.1m, 0);

        decimal shippingFee = request.ShippingFee;
        decimal thanhTienMoi = totalAmount + tienVat + shippingFee;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@HoaDonID", orderId);
        command.Parameters.AddWithValue("@TongTienVAT", tienVat);
        command.Parameters.AddWithValue("@ThanhTien", thanhTienMoi);
        command.Parameters.AddWithValue("@NgayTao", createdAt);
        command.Parameters.AddWithValue("@TrangThai", request.PaymentMethod == "QR" ? 7 : 0);
        command.Parameters.AddWithValue("@GhiChu", "Đơn đặt từ Website bán hàng. ĐC giao: " + request.DiaChiGiaoHang);
        command.Parameters.AddWithValue("@KhachHangID", request.KhachHangID);
        command.Parameters.AddWithValue("@DiaChiID", request.DiaChiID);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertOrderDetailAsync(SqlConnection connection, SqlTransaction transaction, string orderId, (string DetailId, string ProductDetailId, int Quantity, decimal UnitPrice, decimal LineTotal) detail, CancellationToken cancellationToken)
    {
        const string sql = """
        INSERT INTO HoaDonChiTiets (HoaDonChiTietID, HoaDonID, ChiTietSanPhamID, SoLuong, DonGia, MucVAT, TienVAT)
        VALUES (@HoaDonChiTietID, @HoaDonID, @ChiTietSanPhamID, @SoLuong, @DonGia, @MucVAT, @TienVAT);
        """;
        decimal mucVat = 10;
        decimal tienVat = Math.Round(detail.LineTotal * (mucVat / 100m), 0);

        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@HoaDonChiTietID", detail.DetailId);
        command.Parameters.AddWithValue("@HoaDonID", orderId);
        command.Parameters.AddWithValue("@ChiTietSanPhamID", detail.ProductDetailId);
        command.Parameters.AddWithValue("@SoLuong", detail.Quantity);
        command.Parameters.AddWithValue("@DonGia", detail.UnitPrice);
        command.Parameters.AddWithValue("@MucVAT", mucVat);
        command.Parameters.AddWithValue("@TienVAT", tienVat);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task UpdateStockAsync(SqlConnection connection, SqlTransaction transaction, string productDetailId, int quantity, CancellationToken cancellationToken)
    {
        const string sql = "UPDATE ChiTietSanPhams SET SoLuongTonKho = SoLuongTonKho - @SoLuong WHERE ChiTietSanPhamID = @ChiTietSanPhamID;";
        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@SoLuong", quantity);
        command.Parameters.AddWithValue("@ChiTietSanPhamID", productDetailId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}