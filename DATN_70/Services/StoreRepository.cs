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
        try
        {
            const string sql = """
            WITH ActiveGlobalPromo AS (
                SELECT TOP 1 * FROM KhuyenMais
                WHERE MaCode IS NULL AND TrangThai = 1 
                  AND GETDATE() >= NgayApDung AND GETDATE() <= NgayKetThuc
                  AND (SoLuongToiDa = 0 OR SoLuongDaDung < SoLuongToiDa)
            ),
            ActiveProductPromo AS (
                SELECT ksp.SanPhamID, km.LoaiGiamGia, km.GiaTriGiam, km.GiamToiDa,
                       ROW_NUMBER() OVER(PARTITION BY ksp.SanPhamID ORDER BY km.GiaTriGiam DESC) as rn
                FROM KhuyenMaiSanPhams ksp
                INNER JOIN KhuyenMais km ON km.KhuyenMaiID = ksp.KhuyenMaiID
                WHERE km.MaCode IS NOT NULL AND km.TrangThai = 1
                  AND GETDATE() >= km.NgayApDung AND GETDATE() <= km.NgayKetThuc
                  AND (km.SoLuongToiDa = 0 OR km.SoLuongDaDung < km.SoLuongToiDa)
            ),
            VariantPrices AS (
                SELECT 
                    ctsp.SanPhamID,
                    ctsp.GiaNiemYet,
                    ctsp.SoLuongTonKho,
                    (ctsp.GiaNiemYet - TinhGia.MucGiamTotNhat) AS GiaBan,
                    COALESCE(
                        CASE 
                            WHEN TinhGia.NguonGiam = 'SP' AND app.LoaiGiamGia = 1 THEN CAST(app.GiaTriGiam AS DECIMAL(18,2))
                            WHEN TinhGia.NguonGiam = 'SP' AND app.LoaiGiamGia = 0 AND ctsp.GiaNiemYet > 0 THEN CAST((app.GiaTriGiam / ctsp.GiaNiemYet) * 100.0 AS DECIMAL(18,2))
                            WHEN TinhGia.NguonGiam = 'SAN' AND agp.LoaiGiamGia = 1 THEN CAST(agp.GiaTriGiam AS DECIMAL(18,2))
                            WHEN TinhGia.NguonGiam = 'SAN' AND agp.LoaiGiamGia = 0 AND ctsp.GiaNiemYet > 0 THEN CAST((agp.GiaTriGiam / ctsp.GiaNiemYet) * 100.0 AS DECIMAL(18,2))
                            ELSE CAST(0 AS DECIMAL(18,2)) 
                        END, CAST(0 AS DECIMAL(18,2))
                    ) AS PhanTramGiam,
                    COALESCE(IIF(TinhGia.NguonGiam = 'SP', app.LoaiGiamGia, agp.LoaiGiamGia), -1) AS LoaiGiamGia,
                    COALESCE(IIF(TinhGia.NguonGiam = 'SP', app.GiaTriGiam, agp.GiaTriGiam), CAST(0 AS DECIMAL(18,2))) AS GiaTriGiam,
                    COALESCE(IIF(TinhGia.NguonGiam = 'SP', app.GiamToiDa, agp.GiamToiDa), CAST(0 AS DECIMAL(18,2))) AS GiamToiDa
                FROM ChiTietSanPhams ctsp
                LEFT JOIN ActiveGlobalPromo agp ON 1 = 1
                LEFT JOIN ActiveProductPromo app ON app.SanPhamID = ctsp.SanPhamID AND app.rn = 1
                CROSS APPLY (
                    SELECT 
                        CASE WHEN agp.GiaTriGiam IS NULL THEN CAST(0 AS DECIMAL(18,2))
                             WHEN agp.LoaiGiamGia = 0 THEN CAST(agp.GiaTriGiam AS DECIMAL(18,2))
                             WHEN agp.LoaiGiamGia = 1 THEN IIF(agp.GiamToiDa > 0 AND (ctsp.GiaNiemYet * agp.GiaTriGiam / 100.0) > agp.GiamToiDa, CAST(agp.GiamToiDa AS DECIMAL(18,2)), CAST(ROUND(ctsp.GiaNiemYet * agp.GiaTriGiam / 100.0, 0) AS DECIMAL(18,2)))
                        END AS GiamSan,
                        CASE WHEN app.GiaTriGiam IS NULL THEN CAST(0 AS DECIMAL(18,2))
                             WHEN app.LoaiGiamGia = 0 THEN CAST(app.GiaTriGiam AS DECIMAL(18,2))
                             WHEN app.LoaiGiamGia = 1 THEN IIF(app.GiamToiDa > 0 AND (ctsp.GiaNiemYet * app.GiaTriGiam / 100.0) > app.GiamToiDa, CAST(app.GiamToiDa AS DECIMAL(18,2)), CAST(ROUND(ctsp.GiaNiemYet * app.GiaTriGiam / 100.0, 0) AS DECIMAL(18,2)))
                        END AS GiamSP
                ) TinhMucGiam
                CROSS APPLY (
                    SELECT 
                        IIF(TinhMucGiam.GiamSP > TinhMucGiam.GiamSan, TinhMucGiam.GiamSP, TinhMucGiam.GiamSan) AS MucGiamTotNhat,
                        IIF(TinhMucGiam.GiamSP > TinhMucGiam.GiamSan, 'SP', IIF(TinhMucGiam.GiamSan > 0, 'SAN', 'NONE')) AS NguonGiam
                ) TinhGia
            )
            SELECT 
                sp.SanPhamID, 
                sp.Ten, 
                sp.MoTa,
                MIN(vp.GiaBan) AS GiaThapNhat,
                MIN(vp.GiaNiemYet) AS GiaGoc,
                MAX(vp.PhanTramGiam) AS PhanTramGiam,
                COALESCE(MAX(vp.LoaiGiamGia), -1) AS LoaiGiamGia,
                COALESCE(MAX(vp.GiaTriGiam), CAST(0 AS DECIMAL(18,2))) AS GiaTriGiam,
                COALESCE(MAX(vp.GiamToiDa), CAST(0 AS DECIMAL(18,2))) AS GiamToiDa,
                SUM(vp.SoLuongTonKho) AS TongSoLuongTon
            FROM SanPhams sp
            LEFT JOIN VariantPrices vp ON sp.SanPhamID = vp.SanPhamID
            GROUP BY sp.SanPhamID, sp.Ten, sp.MoTa
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
                    SanPhamID = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                    Ten = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    MoTa = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),

                    // Dùng Convert.GetValue để chống mọi lỗi InvalidCastException
                    GiaThapNhat = reader.IsDBNull(3) ? 0m : Convert.ToDecimal(reader.GetValue(3)),
                    GiaGoc = reader.IsDBNull(4) ? 0m : Convert.ToDecimal(reader.GetValue(4)),
                    PhanTramGiam = reader.IsDBNull(5) ? 0m : Convert.ToDecimal(reader.GetValue(5)),
                    LoaiGiamGia = reader.IsDBNull(6) ? -1 : Convert.ToInt32(reader.GetValue(6)),
                    GiaTriGiam = reader.IsDBNull(7) ? 0m : Convert.ToDecimal(reader.GetValue(7)),
                    GiamToiDa = reader.IsDBNull(8) ? 0m : Convert.ToDecimal(reader.GetValue(8)),
                    TongSoLuongTon = reader.IsDBNull(9) ? 0 : Convert.ToInt32(reader.GetValue(9))
                });
            }
            return products;
        }
        catch (Exception ex)
        {
            throw new Exception($"LỖI CỰC KỲ CHI TIẾT TẠI GetProductsAsync: {ex.Message}", ex);
        }
    }

    public async Task<ProductDetailResponse?> GetProductDetailAsync(string productId, CancellationToken cancellationToken)
    {
        try
        {
            const string productSql = """
            SELECT SanPhamID, Ten, MoTa
            FROM SanPhams
            WHERE SanPhamID = @SanPhamID;
            """;

            const string variantSql = """
            WITH ActiveGlobalPromo AS (
                SELECT TOP 1 * FROM KhuyenMais
                WHERE MaCode IS NULL AND TrangThai = 1 
                  AND GETDATE() >= NgayApDung AND GETDATE() <= NgayKetThuc
                  AND (SoLuongToiDa = 0 OR SoLuongDaDung < SoLuongToiDa)
            ),
            ActiveProductPromo AS (
                SELECT ksp.SanPhamID, km.LoaiGiamGia, km.GiaTriGiam, km.GiamToiDa,
                       ROW_NUMBER() OVER(PARTITION BY ksp.SanPhamID ORDER BY km.GiaTriGiam DESC) as rn
                FROM KhuyenMaiSanPhams ksp
                INNER JOIN KhuyenMais km ON km.KhuyenMaiID = ksp.KhuyenMaiID
                WHERE km.MaCode IS NOT NULL AND km.TrangThai = 1
                  AND GETDATE() >= km.NgayApDung AND GETDATE() <= km.NgayKetThuc
                  AND (km.SoLuongToiDa = 0 OR km.SoLuongDaDung < km.SoLuongToiDa)
            )
            SELECT 
                ctsp.ChiTietSanPhamID, 
                ctsp.KichCoID, 
                kc.Ten, 
                ctsp.MauID, 
                m.Ten,
                CAST((ctsp.GiaNiemYet - TinhGia.MucGiamTotNhat) AS DECIMAL(18,2)) AS GiaBan,
                CAST(ctsp.GiaNiemYet AS DECIMAL(18,2)) AS GiaGoc,
                COALESCE(
                    CASE 
                        WHEN TinhGia.NguonGiam = 'SP' AND app.LoaiGiamGia = 1 THEN CAST(app.GiaTriGiam AS DECIMAL(18,2))
                        WHEN TinhGia.NguonGiam = 'SP' AND app.LoaiGiamGia = 0 AND ctsp.GiaNiemYet > 0 THEN CAST((app.GiaTriGiam / ctsp.GiaNiemYet) * 100.0 AS DECIMAL(18,2))
                        WHEN TinhGia.NguonGiam = 'SAN' AND agp.LoaiGiamGia = 1 THEN CAST(agp.GiaTriGiam AS DECIMAL(18,2))
                        WHEN TinhGia.NguonGiam = 'SAN' AND agp.LoaiGiamGia = 0 AND ctsp.GiaNiemYet > 0 THEN CAST((agp.GiaTriGiam / ctsp.GiaNiemYet) * 100.0 AS DECIMAL(18,2))
                        ELSE CAST(0 AS DECIMAL(18,2)) 
                    END, CAST(0 AS DECIMAL(18,2))
                ) AS PhanTramGiam,
                COALESCE(IIF(TinhGia.NguonGiam = 'SP', app.LoaiGiamGia, agp.LoaiGiamGia), -1) AS LoaiGiamGia,
                COALESCE(IIF(TinhGia.NguonGiam = 'SP', app.GiaTriGiam, agp.GiaTriGiam), CAST(0 AS DECIMAL(18,2))) AS GiaTriGiam,
                COALESCE(IIF(TinhGia.NguonGiam = 'SP', app.GiamToiDa, agp.GiamToiDa), CAST(0 AS DECIMAL(18,2))) AS GiamToiDa,
                ctsp.SoLuongTonKho
            FROM ChiTietSanPhams ctsp
            INNER JOIN KichCos kc ON kc.KichCoID = ctsp.KichCoID
            INNER JOIN Maus m ON m.MauID = ctsp.MauID
            LEFT JOIN ActiveGlobalPromo agp ON 1 = 1
            LEFT JOIN ActiveProductPromo app ON app.SanPhamID = ctsp.SanPhamID AND app.rn = 1
            CROSS APPLY (
                SELECT 
                    CASE WHEN agp.GiaTriGiam IS NULL THEN CAST(0 AS DECIMAL(18,2))
                         WHEN agp.LoaiGiamGia = 0 THEN CAST(agp.GiaTriGiam AS DECIMAL(18,2))
                         WHEN agp.LoaiGiamGia = 1 THEN IIF(agp.GiamToiDa > 0 AND (ctsp.GiaNiemYet * agp.GiaTriGiam / 100.0) > agp.GiamToiDa, CAST(agp.GiamToiDa AS DECIMAL(18,2)), CAST(ROUND(ctsp.GiaNiemYet * agp.GiaTriGiam / 100.0, 0) AS DECIMAL(18,2)))
                    END AS GiamSan,
                    CASE WHEN app.GiaTriGiam IS NULL THEN CAST(0 AS DECIMAL(18,2))
                         WHEN app.LoaiGiamGia = 0 THEN CAST(app.GiaTriGiam AS DECIMAL(18,2))
                         WHEN app.LoaiGiamGia = 1 THEN IIF(app.GiamToiDa > 0 AND (ctsp.GiaNiemYet * app.GiaTriGiam / 100.0) > app.GiamToiDa, CAST(app.GiamToiDa AS DECIMAL(18,2)), CAST(ROUND(ctsp.GiaNiemYet * app.GiaTriGiam / 100.0, 0) AS DECIMAL(18,2)))
                    END AS GiamSP
            ) TinhMucGiam
            CROSS APPLY (
                SELECT 
                    IIF(TinhMucGiam.GiamSP > TinhMucGiam.GiamSan, TinhMucGiam.GiamSP, TinhMucGiam.GiamSan) AS MucGiamTotNhat,
                    IIF(TinhMucGiam.GiamSP > TinhMucGiam.GiamSan, 'SP', IIF(TinhMucGiam.GiamSan > 0, 'SAN', 'NONE')) AS NguonGiam
            ) TinhGia
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
                    ChiTietSanPhamID = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                    KichCoID = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    TenKichCo = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    MauID = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    TenMau = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),

                    // 🚀 Sử dụng Convert chống lỗi lệch pha dữ liệu ép kiểu tuyệt đối
                    GiaNiemYet = reader.IsDBNull(5) ? 0m : Convert.ToDecimal(reader.GetValue(5)),
                    GiaGoc = reader.IsDBNull(6) ? 0m : Convert.ToDecimal(reader.GetValue(6)),
                    PhanTramGiam = reader.IsDBNull(7) ? 0m : Convert.ToDecimal(reader.GetValue(7)),
                    LoaiGiamGia = reader.IsDBNull(8) ? -1 : Convert.ToInt32(reader.GetValue(8)),
                    GiaTriGiam = reader.IsDBNull(9) ? 0m : Convert.ToDecimal(reader.GetValue(9)),
                    GiamToiDa = reader.IsDBNull(10) ? 0m : Convert.ToDecimal(reader.GetValue(10)),
                    SoLuongTon = reader.IsDBNull(11) ? 0 : Convert.ToInt32(reader.GetValue(11))
                });
            }

            return product;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi GetProductDetailAsync: {ex.Message}");
            return null;
        }
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
            decimal totalAmount = 0; // Tiền hàng chưa VAT
            decimal tongTienVat = 0;
            var orderDetails = new List<(string DetailId, string ProductDetailId, int Quantity, decimal UnitPrice, decimal LineTotal, int MucVAT)>();

            foreach (var item in normalizedItems)
            {
                var stockInfo = await GetStockInfoAsync(connection, transaction, item.ChiTietSanPhamID, cancellationToken);
                if (stockInfo is null) { await transaction.RollbackAsync(cancellationToken); return ServiceResult<OrderCreatedResponse>.Fail($"Khong tim thay bien the {item.ChiTietSanPhamID}."); }

                var (giaBan, soLuongTon, mucVAT) = stockInfo.Value;
                if (soLuongTon < item.SoLuong) { await transaction.RollbackAsync(cancellationToken); return ServiceResult<OrderCreatedResponse>.Fail($"San pham {item.ChiTietSanPhamID} chi con {soLuongTon} trong kho."); }

                var detailId = GenerateId("HDCT", 20);
                var lineTotal = giaBan * item.SoLuong;
                totalAmount += lineTotal;

                // Tính VAT cho dòng này
                var tienVatItem = Math.Round(lineTotal * mucVAT / 100m, 0);
                tongTienVat += tienVatItem;

                orderDetails.Add((detailId, item.ChiTietSanPhamID, item.SoLuong, giaBan, lineTotal, mucVAT));
            }

            await InsertOrderAsync(connection, transaction, orderId, request, orderCreatedAt, totalAmount, tongTienVat, cancellationToken);

            foreach (var detail in orderDetails)
            {
                await InsertOrderDetailAsync(connection, transaction, orderId, detail, cancellationToken);
                if (request.PaymentMethod != "QR")
                {
                    await UpdateStockAsync(connection, transaction, detail.ProductDetailId, detail.Quantity, cancellationToken);
                }
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

    private static async Task<(decimal GiaBan, int SoLuongTon, int MucVAT)?> GetStockInfoAsync(SqlConnection connection, SqlTransaction transaction, string productDetailId, CancellationToken cancellationToken)
    {
        try
        {
            const string sql = """
SELECT
    CASE
        WHEN agp.GiaTriGiam IS NOT NULL THEN
            CASE
                WHEN agp.LoaiGiamGia = 0 THEN IIF(ctsp.GiaNiemYet - agp.GiaTriGiam < 0, 0, ctsp.GiaNiemYet - agp.GiaTriGiam)
                WHEN agp.LoaiGiamGia = 1 THEN
                    CASE WHEN agp.GiamToiDa > 0 AND (ctsp.GiaNiemYet * agp.GiaTriGiam / 100.0) > agp.GiamToiDa
                         THEN ctsp.GiaNiemYet - agp.GiamToiDa
                         ELSE ctsp.GiaNiemYet - ROUND(ctsp.GiaNiemYet * agp.GiaTriGiam / 100.0, 0)
                    END
                ELSE ctsp.GiaNiemYet
            END
        WHEN app.GiaTriGiam IS NOT NULL THEN
            CASE
                WHEN app.LoaiGiamGia = 0 THEN IIF(ctsp.GiaNiemYet - app.GiaTriGiam < 0, 0, ctsp.GiaNiemYet - app.GiaTriGiam)
                WHEN app.LoaiGiamGia = 1 THEN
                    CASE WHEN app.GiamToiDa > 0 AND (ctsp.GiaNiemYet * app.GiaTriGiam / 100.0) > app.GiamToiDa
                         THEN ctsp.GiaNiemYet - app.GiamToiDa
                         ELSE ctsp.GiaNiemYet - ROUND(ctsp.GiaNiemYet * app.GiaTriGiam / 100.0, 0)
                    END
                ELSE ctsp.GiaNiemYet
            END
        ELSE ctsp.GiaNiemYet
    END AS GiaBan,
    ctsp.SoLuongTonKho,
    sp.MucVAT
FROM ChiTietSanPhams ctsp WITH (UPDLOCK, ROWLOCK)
INNER JOIN SanPhams sp ON ctsp.SanPhamID = sp.SanPhamID
OUTER APPLY (
    SELECT TOP 1
        km.LoaiGiamGia,
        km.GiaTriGiam,
        km.GiamToiDa
    FROM KhuyenMaiSanPhams ksp
    INNER JOIN KhuyenMais km ON km.KhuyenMaiID = ksp.KhuyenMaiID
    WHERE ksp.SanPhamID = ctsp.SanPhamID
      AND km.MaCode IS NOT NULL
      AND km.TrangThai = 1
      AND GETDATE() >= km.NgayApDung
      AND GETDATE() <= km.NgayKetThuc
      AND (km.SoLuongToiDa = 0 OR km.SoLuongDaDung < km.SoLuongToiDa)
    ORDER BY km.GiaTriGiam DESC
) app
OUTER APPLY (
    SELECT TOP 1 *
    FROM KhuyenMais
    WHERE MaCode IS NULL
      AND TrangThai = 1
      AND GETDATE() >= NgayApDung
      AND GETDATE() <= NgayKetThuc
      AND (SoLuongToiDa = 0 OR SoLuongDaDung < SoLuongToiDa)
) agp
WHERE ctsp.ChiTietSanPhamID = @ChiTietSanPhamID;
""";

            await using var command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@ChiTietSanPhamID", productDetailId);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken)) return null;
            return (reader.GetDecimal(0), reader.GetInt32(1), reader.GetInt32(2));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi GetStockInfoAsync: {ex.Message}");
            return null;
        }
    }

    private static async Task InsertOrderAsync(SqlConnection connection, SqlTransaction transaction, string orderId, PlaceOrderRequest request, DateTime createdAt, decimal totalAmount, decimal tongTienVat, CancellationToken cancellationToken)
    {
        const string sql = @"
INSERT INTO HoaDons (HoaDonID, TongTienVAT, TongTienGiamGia, ThanhTien, LoaiGiaoDich, NgayTao, TrangThai, GhiChu, KhachHangID, NhanVienID, DiaChiID, KhuyenMaiID)
VALUES (@HoaDonID, @TongTienVAT, 0, @ThanhTien, 0, @NgayTao, @TrangThai, @GhiChu, @KhachHangID, N'NV0001', @DiaChiID, NULL)";

        decimal shippingFee = request.ShippingFee;
        decimal thanhTienMoi = totalAmount + tongTienVat + shippingFee;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@HoaDonID", orderId);
        command.Parameters.AddWithValue("@TongTienVAT", tongTienVat);
        command.Parameters.AddWithValue("@ThanhTien", thanhTienMoi);
        command.Parameters.AddWithValue("@NgayTao", createdAt);
        command.Parameters.AddWithValue("@TrangThai", request.PaymentMethod == "QR" ? 7 : 0);
        command.Parameters.AddWithValue("@GhiChu", "Đơn đặt từ Website bán hàng. ĐC giao: " + request.DiaChiGiaoHang);
        command.Parameters.AddWithValue("@KhachHangID", request.KhachHangID);
        command.Parameters.AddWithValue("@DiaChiID", request.DiaChiID);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task InsertOrderDetailAsync(SqlConnection connection, SqlTransaction transaction, string orderId, (string DetailId, string ProductDetailId, int Quantity, decimal UnitPrice, decimal LineTotal, int MucVAT) detail, CancellationToken cancellationToken)
    {
        const string sql = """
        INSERT INTO HoaDonChiTiets (HoaDonChiTietID, HoaDonID, ChiTietSanPhamID, SoLuong, DonGia, MucVAT, TienVAT)
        VALUES (@HoaDonChiTietID, @HoaDonID, @ChiTietSanPhamID, @SoLuong, @DonGia, @MucVAT, @TienVAT);
        """;

        decimal tienVat = Math.Round(detail.LineTotal * (detail.MucVAT / 100m), 0);

        await using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddWithValue("@HoaDonChiTietID", detail.DetailId);
        command.Parameters.AddWithValue("@HoaDonID", orderId);
        command.Parameters.AddWithValue("@ChiTietSanPhamID", detail.ProductDetailId);
        command.Parameters.AddWithValue("@SoLuong", detail.Quantity);
        command.Parameters.AddWithValue("@DonGia", detail.UnitPrice);
        command.Parameters.AddWithValue("@MucVAT", detail.MucVAT);
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