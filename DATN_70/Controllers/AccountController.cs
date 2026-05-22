using DATN_70.Data;
using DATN_70.Models.Entities;
using DATN_70.Models.Enums;
using DATN_70.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN_70.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _dbContext;

    public AccountController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(TaiKhoan model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await _dbContext.TaiKhoans.AnyAsync(item => item.Email == model.Email))
        {
            ModelState.AddModelError(nameof(TaiKhoan.Email), "Email này đã được sử dụng.");
            return View(model);
        }

        model.TaiKhoanID = string.IsNullOrWhiteSpace(model.TaiKhoanID) ? Guid.NewGuid().ToString() : model.TaiKhoanID;
        model.TrangThai = string.IsNullOrWhiteSpace(model.TrangThai) ? "Hoạt động" : model.TrangThai;
        model.VaiTroID = string.IsNullOrWhiteSpace(model.VaiTroID) ? "R03" : model.VaiTroID;

        var customer = new KhachHang
        {
            KhachHangID = GenerateId("KH", 20),
            Ten = model.Email.Split('@')[0],
            Email = model.Email,
            SoDienThoai = "0000000000",
            GioiTinh = Enums.GioiTinh.Khac,
            DiaChi = string.Empty,
            TaiKhoanID = model.TaiKhoanID
        };

        _dbContext.TaiKhoans.Add(model);
        _dbContext.KhachHangs.Add(customer);
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        // ĐỒNG BỘ: Bắt buộc phải .Include(item => item.NhanVien) để bốc được thông tin nhân viên lên cùng tài khoản
        var user = await _dbContext.TaiKhoans
            .Include(item => item.KhachHang)
            .Include(item => item.NhanVien)
            .FirstOrDefaultAsync(item => item.Email == email && item.MatKhau == password);

        if (user is null)
        {
            ViewBag.Error = "Sai tài khoản hoặc mật khẩu.";
            return View();
        }

        // 1. Lưu các thông tin tài khoản cơ bản như cũ
        HttpContext.Session.SetString("UserId", user.TaiKhoanID);
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("UserRole", user.VaiTroID); // Phục vụ phân quyền sau này (R01, R02, R03)

        // 2. TRÁI NGỌT Ở ĐÂY: Nếu tài khoản này có gắn với nhân viên, lưu ngay mã NhanVienID (NV...) vào Session
        if (user.NhanVien != null)
        {
            HttpContext.Session.SetString("NhanVienId", user.NhanVien.NhanVienID);
        }

        if (user.KhachHang is null)
        {
            _dbContext.KhachHangs.Add(new KhachHang
            {
                KhachHangID = GenerateId("KH", 20),
                Ten = user.Email.Split('@')[0],
                Email = user.Email,
                SoDienThoai = "0000000000",
                GioiTinh = Enums.GioiTinh.Khac,
                DiaChi = string.Empty,
                TaiKhoanID = user.TaiKhoanID
            });
            await _dbContext.SaveChangesAsync();
        }

        // Phân quyền sau đăng nhập
        if (user.VaiTroID == "R01" || user.VaiTroID == "R02")
        {
            return RedirectToAction("Dashboard", "Admin");
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }

    // BỔ SUNG: API Trả về trang báo lỗi 403 nếu cố tình hack URL vào trang Admin
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
    [HttpGet]
    public IActionResult Index()
    {
        if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("UserId")))
        {
            return RedirectToAction(nameof(Login));
        }
        // Trả về trang khung chứa Menu bên trái và hộp trống bên phải
        return View();
    }
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await GetCurrentAccountAsync();
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var defaultAddress = await _dbContext.DiaChis
            .AsNoTracking()
            .Where(item => item.KhachHangID == user.KhachHang!.KhachHangID)
            .OrderByDescending(item => item.LaMacDinh)
            .Select(item => new
            {
                item.TinhThanh,
                item.QuanHuyen,
                item.PhuongXa
            })
            .FirstOrDefaultAsync();

        var model = new AccountProfileViewModel
        {
            TaiKhoanId = user.TaiKhoanID,
            KhachHangId = user.KhachHang!.KhachHangID,
            FullName = user.KhachHang.Ten,
            Email = user.Email,
            Phone = NormalizePhoneForDisplay(user.KhachHang.SoDienThoai),
            DefaultAddressText = defaultAddress is null
                ? "Chưa có địa chỉ mặc định."
                : string.Join(", ", new[]
                {
                    AddressSerializer.ExtractStreet(defaultAddress.PhuongXa),
                    AddressSerializer.ExtractWard(defaultAddress.PhuongXa),
                    defaultAddress.QuanHuyen,
                    defaultAddress.TinhThanh
                }.Where(part => !string.IsNullOrWhiteSpace(part)))
        };

        if (TempData["ProfileStatus"] is string statusMessage)
        {
            model.StatusMessage = statusMessage;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(AccountProfileViewModel model)
    {
        var user = await GetCurrentAccountAsync();
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (!ModelState.IsValid)
        {
            model.DefaultAddressText = await GetDefaultAddressTextAsync(user.KhachHang!.KhachHangID);
            return View(model);
        }

        user.Email = model.Email.Trim();
        user.KhachHang!.Ten = model.FullName.Trim();
        user.KhachHang.Email = model.Email.Trim();
        user.KhachHang.SoDienThoai = NormalizePhoneForStorage(model.Phone);

        await _dbContext.SaveChangesAsync();

        HttpContext.Session.SetString("UserEmail", user.Email);
        TempData["ProfileStatus"] = "Thông tin tài khoản đã được cập nhật.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public async Task<IActionResult> Addresses(string? editId = null)
    {
        var user = await GetCurrentAccountAsync();
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var addresses = await BuildAddressItemsAsync(user.KhachHang!.KhachHangID);
        var editing = addresses.FirstOrDefault(item => item.Id == editId);

        var model = new AccountAddressPageViewModel
        {
            Addresses = addresses,
            Form = editing is null
                ? new AddressFormViewModel()
                : new AddressFormViewModel
                {
                    Id = editing.Id,
                    RecipientName = editing.RecipientName,
                    Phone = editing.Phone,
                    Street = editing.Street,
                    Province = editing.Province,
                    District = editing.District,
                    Ward = editing.Ward,
                    IsDefault = editing.IsDefault
                }
        };

        if (TempData["AddressStatus"] is string statusMessage)
        {
            model.StatusMessage = statusMessage;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAddress(AddressFormViewModel form)
    {
        var user = await GetCurrentAccountAsync();
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (!ModelState.IsValid)
        {
            var invalidModel = new AccountAddressPageViewModel
            {
                Form = form,
                Addresses = await BuildAddressItemsAsync(user.KhachHang!.KhachHangID)
            };
            return View("Addresses", invalidModel);
        }

        var customer = user.KhachHang!;
        var normalizedPhone = NormalizePhoneForStorage(form.Phone);
        var address = string.IsNullOrWhiteSpace(form.Id)
            ? new DiaChi
            {
                DiaChiID = Guid.NewGuid().ToString(),
                KhachHangID = customer.KhachHangID
            }
            : await _dbContext.DiaChis.FirstOrDefaultAsync(item => item.DiaChiID == form.Id && item.KhachHangID == customer.KhachHangID);

        if (address is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(form.Id))
        {
            _dbContext.DiaChis.Add(address);
        }

        address.TenNguoiNhan = form.RecipientName.Trim();
        address.SoDienThoaiNhan = normalizedPhone;
        address.TinhThanh = form.Province.Trim();
        address.QuanHuyen = form.District.Trim();
        address.PhuongXa = AddressSerializer.Pack(form.Ward.Trim(), form.Street.Trim());
        address.LaMacDinh = form.IsDefault;

        if (form.IsDefault)
        {
            await ClearDefaultAddressesAsync(customer.KhachHangID, address.DiaChiID);
        }

        customer.DiaChi = string.Join(", ", new[] { form.Street.Trim(), form.Ward.Trim(), form.District.Trim(), form.Province.Trim() });
        customer.SoDienThoai = normalizedPhone;
        if (!string.IsNullOrWhiteSpace(form.RecipientName))
        {
            customer.Ten = form.RecipientName.Trim();
        }

        await _dbContext.SaveChangesAsync();

        TempData["AddressStatus"] = string.IsNullOrWhiteSpace(form.Id)
            ? "Địa chỉ mới đã được thêm."
            : "Địa chỉ đã được cập nhật.";
        return RedirectToAction(nameof(Addresses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefaultAddress(string id)
    {
        var user = await GetCurrentAccountAsync();
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var address = await _dbContext.DiaChis.FirstOrDefaultAsync(item => item.DiaChiID == id && item.KhachHangID == user.KhachHang!.KhachHangID);
        if (address is null)
        {
            return NotFound();
        }

        await ClearDefaultAddressesAsync(user.KhachHang!.KhachHangID, address.DiaChiID);
        address.LaMacDinh = true;
        user.KhachHang.DiaChi = string.Join(", ", new[]
        {
            AddressSerializer.ExtractStreet(address.PhuongXa),
            AddressSerializer.ExtractWard(address.PhuongXa),
            address.QuanHuyen,
            address.TinhThanh
        }.Where(part => !string.IsNullOrWhiteSpace(part)));
        user.KhachHang.SoDienThoai = address.SoDienThoaiNhan;
        user.KhachHang.Ten = address.TenNguoiNhan;

        await _dbContext.SaveChangesAsync();
        TempData["AddressStatus"] = "Đã cập nhật địa chỉ mặc định.";
        return RedirectToAction(nameof(Addresses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAddress(string id)
    {
        var user = await GetCurrentAccountAsync();
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var address = await _dbContext.DiaChis.FirstOrDefaultAsync(item => item.DiaChiID == id && item.KhachHangID == user.KhachHang!.KhachHangID);
        if (address is null)
        {
            return NotFound();
        }

        _dbContext.DiaChis.Remove(address);
        await _dbContext.SaveChangesAsync();

        TempData["AddressStatus"] = "Địa chỉ đã được xóa.";
        return RedirectToAction(nameof(Addresses));
    }

    [HttpGet]
    public IActionResult Password()
    {
        if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("UserId")))
        {
            return RedirectToAction(nameof(Login));
        }

        var model = new ChangePasswordViewModel();
        if (TempData["PasswordStatus"] is string statusMessage)
        {
            model.StatusMessage = statusMessage;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Password(ChangePasswordViewModel model)
    {
        var user = await GetCurrentAccountAsync();
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!string.Equals(user.MatKhau, model.CurrentPassword, StringComparison.Ordinal))
        {
            ModelState.AddModelError(nameof(ChangePasswordViewModel.CurrentPassword), "Mật khẩu hiện tại không đúng.");
            return View(model);
        }

        user.MatKhau = model.NewPassword;
        await _dbContext.SaveChangesAsync();

        TempData["PasswordStatus"] = "Mật khẩu đã được cập nhật.";
        return RedirectToAction(nameof(Password));
    }
    // GET: Hiển thị form quên mật khẩu
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    // POST: Xử lý yêu cầu quên mật khẩu
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _dbContext.TaiKhoans.FirstOrDefaultAsync(t => t.Email == model.Email);
        if (user == null)
        {
            // Vẫn báo thành công để không lộ thông tin user
            TempData["ForgotPasswordStatus"] = "Nếu email tồn tại, link đặt lại mật khẩu sẽ được hiển thị.";
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        // Tạo token và lưu vào DB
        user.ResetToken = Guid.NewGuid().ToString("N");
        user.ResetTokenExpiry = DateTime.Now.AddMinutes(15);
        await _dbContext.SaveChangesAsync();

        // Tạo link reset (giả lập gửi email bằng cách hiển thị trên màn hình)
        var resetLink = Url.Action("ResetPassword", "Account", new { token = user.ResetToken }, Request.Scheme);
        ViewBag.ResetLink = resetLink;

        return View("ForgotPasswordConfirmation");
    }

    // GET: Hiển thị trang xác nhận đã gửi link
    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    // GET: Hiển thị form đặt mật khẩu mới
    [HttpGet]
    public async Task<IActionResult> ResetPassword(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Login");

        var user = await _dbContext.TaiKhoans.FirstOrDefaultAsync(t => t.ResetToken == token && t.ResetTokenExpiry > DateTime.Now);
        if (user == null)
        {
            TempData["ResetPasswordError"] = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
            return RedirectToAction("Login");
        }

        var model = new ResetPasswordViewModel { Token = token };
        return View(model);
    }

    // POST: Xử lý đặt mật khẩu mới
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _dbContext.TaiKhoans.FirstOrDefaultAsync(t => t.ResetToken == model.Token && t.ResetTokenExpiry > DateTime.Now);
        if (user == null)
        {
            ModelState.AddModelError("", "Link không hợp lệ hoặc đã hết hạn.");
            return View(model);
        }

        user.MatKhau = model.NewPassword;
        user.ResetToken = null;
        user.ResetTokenExpiry = null;
        await _dbContext.SaveChangesAsync();

        TempData["LoginStatus"] = "Mật khẩu đã được đặt lại thành công. Vui lòng đăng nhập.";
        return RedirectToAction("Login");
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(string id)
    {
        var user = await GetCurrentAccountAsync();
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        // Nạp kèm chi tiết đơn hàng để có thể cộng lại tồn kho
        var order = await _dbContext.HoaDons
            .Include(h => h.HoaDonChiTiets)
            .FirstOrDefaultAsync(item => item.HoaDonID == id && item.KhachHangID == user.KhachHang!.KhachHangID);

        if (order is null)
        {
            return NotFound();
        }

        // Cho phép hủy nếu đơn ở trạng thái: Chờ xác nhận (0), Đã xác nhận (1), hoặc Đang chuẩn bị (2)
        // Tức là bất kỳ trạng thái nào nhỏ hơn "Đang giao" (3)
        if (order.TrangThai < Enums.TrangThaiHoaDon.DangGiao || order.TrangThai == Enums.TrangThaiHoaDon.DangChoThanhToanQR)
        {
            // *** PHẦN XỬ LÝ HOÀN TRẢ TỒN KHO MỚI THÊM VÀO ***
            // Chỉ cộng lại tồn kho cho đơn hàng đã từng bị trừ (COD hoặc QR đã thanh toán)
            if (order.TrangThai != Enums.TrangThaiHoaDon.DangChoThanhToanQR) // Loại trừ đơn QR chưa thanh toán (trạng thái 7)
            {
                foreach (var detail in order.HoaDonChiTiets)
                {
                    var chiTietSP = await _dbContext.ChiTietSanPhams
                        .FirstOrDefaultAsync(ct => ct.ChiTietSanPhamID == detail.ChiTietSanPhamID);
                    if (chiTietSP != null)
                    {
                        chiTietSP.SoLuongTonKho += detail.SoLuong;
                    }
                }
            }

            order.TrangThai = Enums.TrangThaiHoaDon.DaHuy; // Chuyển sang Đã hủy (5)
            await _dbContext.SaveChangesAsync();
            TempData["OrderStatusMessage"] = $"Đã hủy đơn hàng {id} thành công.";
        }
        else
        {
            TempData["OrderErrorMessage"] = "Không thể hủy đơn hàng do đơn đã được bàn giao cho đơn vị vận chuyển.";
        }

        return RedirectToAction(nameof(Orders));
    }
    public async Task<IActionResult> SubmitReturnRequest([FromBody] CustomerReturnRequest request)
    {
        var user = await GetCurrentAccountAsync();
        if (user == null) return Unauthorized(new { message = "Phiên đăng nhập đã hết hạn." });

        // 1. Kiểm tra đơn hàng có đúng của khách này không và có đang ở trạng thái Thành công không
        var order = await _dbContext.HoaDons
            .Include(h => h.HoaDonChiTiets)
            .FirstOrDefaultAsync(h => h.HoaDonID == request.HoaDonId && h.KhachHangID == user.KhachHang!.KhachHangID);

        if (order == null) return NotFound(new { message = "Không tìm thấy hóa đơn." });
        if (order.TrangThai != Enums.TrangThaiHoaDon.HoanThanh) return BadRequest(new { message = "Chỉ những đơn hàng đã giao Thành công mới được phép đổi/trả." });

        // 2. Chặn nếu đơn này đã từng tạo phiếu RMA trước đó
        bool hasRMA = await _dbContext.Set<PhieuDoiTra>().AnyAsync(p => p.HoaDonID == request.HoaDonId);
        if (hasRMA) return BadRequest(new { message = "Đơn hàng này đã có yêu cầu đổi trả đang được xử lý trên hệ thống." });

        var phieuId = "RMA" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        decimal tongTienHoan = 0;
        var chiTietPhieus = new List<ChiTietDoiTra>();

        foreach (var item in request.Items)
        {
            var hdBf = order.HoaDonChiTiets.FirstOrDefault(d => d.ChiTietSanPhamID == item.ChiTietSanPhamId);
            if (hdBf == null || item.SoLuongTra > hdBf.SoLuong)
                return BadRequest(new { message = "Số lượng hoàn trả không hợp lệ!" });

            decimal giaTriHoanItem = hdBf.DonGia * item.SoLuongTra;
            tongTienHoan += giaTriHoanItem;

            chiTietPhieus.Add(new ChiTietDoiTra
            {
                ChiTietDoiTraID = Guid.NewGuid().ToString(),
                PhieuDoiTraID = phieuId,
                ChiTietSanPhamID = item.ChiTietSanPhamId,
                SoLuongTra = item.SoLuongTra,
                GiaTriHoanLai = giaTriHoanItem,
                LyDo = (Enums.LyDoDoiTra)item.LyDoKey
            });
        }

        var phieuDoiTra = new PhieuDoiTra
        {
            PhieuDoiTraID = phieuId,
            HoaDonID = request.HoaDonId,
            NgayTao = DateTime.Now,
            TrangThai = Enums.TrangThaiDoiTra.ChoXuLy, // Mặc định chuyển sang trạng thái chờ Admin duyệt
            TongTienHoan = tongTienHoan,
            GhiChuAdmin = "Khách hàng ghi chú: " + (request.GhiChu ?? "")
        };

        _dbContext.Set<PhieuDoiTra>().Add(phieuDoiTra);
        _dbContext.Set<ChiTietDoiTra>().AddRange(chiTietPhieus);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Gửi yêu cầu đổi trả thành công! Vui lòng chờ nhân viên liên hệ xác nhận." });
    }
    [HttpGet]
    public async Task<IActionResult> Orders(string status = "all")
    {
        var user = await GetCurrentAccountAsync();
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var query = _dbContext.HoaDons
            .AsNoTracking()
            .Include(item => item.DiaChi)
            .Include(item => item.HoaDonChiTiets)
                .ThenInclude(item => item.ChiTietSanPham)
                    .ThenInclude(item => item.SanPham)
            .Where(item => item.KhachHangID == user.KhachHang!.KhachHangID);

        if (TryMapOrderFilter(status, out var orderStatus))
        {
            query = query.Where(item => item.TrangThai == orderStatus);
        }

        var orderEntities = await query
            .OrderByDescending(item => item.NgayTao)
            .ToListAsync();

        // ĐỒNG BỘ: Quét toàn bộ phiếu Đổi/Trả RMA của khách hàng này để tìm trạng thái trung gian
        var orderIds = orderEntities.Select(o => o.HoaDonID).ToList();
        var rmaRequests = await _dbContext.Set<PhieuDoiTra>()
            .AsNoTracking()
            .Where(p => orderIds.Contains(p.HoaDonID))
            .ToListAsync();

        var orders = orderEntities.Select(item => {
            // Lấy key và label gốc của hóa đơn
            var statusKey = GetStatusKey(item.TrangThai);
            var statusLabel = GetStatusLabel(item.TrangThai);

            // LOGIC THÔNG MINH: Nếu đơn thành công nhưng đang có khiếu nại đổi trả, ép hiển thị trạng thái trung gian
            if (item.TrangThai == Enums.TrangThaiHoaDon.HoanThanh)
            {
                var rma = rmaRequests.FirstOrDefault(p => p.HoaDonID == item.HoaDonID);
                if (rma != null)
                {
                    if (rma.TrangThai == Enums.TrangThaiDoiTra.ChoXuLy)
                    {
                        statusKey = "return_pending";
                        statusLabel = "Đang chờ duyệt đổi/trả";
                    }
                    else if (rma.TrangThai == Enums.TrangThaiDoiTra.TuChoi)
                    {
                        statusKey = "return_rejected";
                        statusLabel = "Bị từ chối đổi/trả";
                    }
                }
            }

            return new AccountOrderViewModel
            {
                Id = item.HoaDonID,
                StatusKey = statusKey,
                StatusLabel = statusLabel,
                CreatedAt = item.NgayTao,
                TotalAmount = (decimal)item.ThanhTien,
                RecipientName = item.DiaChi != null ? item.DiaChi.TenNguoiNhan : user.KhachHang!.Ten,
                Phone = item.DiaChi != null ? item.DiaChi.SoDienThoaiNhan : user.KhachHang!.SoDienThoai,
                ShippingAddress = item.DiaChi != null
                    ? string.Join(", ", new[]
                    {
                        AddressSerializer.ExtractStreet(item.DiaChi.PhuongXa),
                        AddressSerializer.ExtractWard(item.DiaChi.PhuongXa),
                        item.DiaChi.QuanHuyen,
                        item.DiaChi.TinhThanh
                    }.Where(part => !string.IsNullOrWhiteSpace(part)))
                    : user.KhachHang!.DiaChi,
                Items = item.HoaDonChiTiets.Select(detail => new AccountOrderLineViewModel
                {
                    ChiTietSanPhamId = detail.ChiTietSanPhamID,
                    ProductName = detail.ChiTietSanPham.SanPham.Ten,
                    Variant = $"{detail.ChiTietSanPham.KichCoID} / {detail.ChiTietSanPham.MauID}",
                    Quantity = detail.SoLuong,
                    UnitPrice = detail.DonGia
                }).ToList()
            };
        }).ToList();

        return View(new AccountOrdersPageViewModel
        {
            CurrentFilter = status,
            Orders = orders
        });
    }

    private async Task<TaiKhoan?> GetCurrentAccountAsync()
    {
        var userId = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var user = await _dbContext.TaiKhoans
            .Include(item => item.KhachHang)
            .FirstOrDefaultAsync(item => item.TaiKhoanID == userId);

        if (user?.KhachHang is not null)
        {
            return user;
        }

        if (user is null)
        {
            return null;
        }

        var customer = new KhachHang
        {
            KhachHangID = GenerateId("KH", 20),
            Ten = user.Email.Split('@')[0],
            Email = user.Email,
            SoDienThoai = "0000000000",
            GioiTinh = Enums.GioiTinh.Khac,
            DiaChi = string.Empty,
            TaiKhoanID = user.TaiKhoanID
        };

        _dbContext.KhachHangs.Add(customer);
        await _dbContext.SaveChangesAsync();
        user.KhachHang = customer;
        return user;
    }

    private async Task<List<AccountAddressItemViewModel>> BuildAddressItemsAsync(string customerId)
    {
        var entities = await _dbContext.DiaChis
            .AsNoTracking()
            .Where(item => item.KhachHangID == customerId)
            .OrderByDescending(item => item.LaMacDinh)
            .ThenBy(item => item.TenNguoiNhan)
            .ToListAsync();

        return entities.Select(item => new AccountAddressItemViewModel
        {
            Id = item.DiaChiID,
            RecipientName = item.TenNguoiNhan,
            Phone = item.SoDienThoaiNhan,
            Street = AddressSerializer.ExtractStreet(item.PhuongXa),
            Ward = AddressSerializer.ExtractWard(item.PhuongXa),
            District = item.QuanHuyen,
            Province = item.TinhThanh,
            IsDefault = item.LaMacDinh
        }).ToList();
    }

    private async Task<string> GetDefaultAddressTextAsync(string customerId)
    {
        var address = await _dbContext.DiaChis
            .AsNoTracking()
            .Where(item => item.KhachHangID == customerId)
            .OrderByDescending(item => item.LaMacDinh)
            .Select(item => new
            {
                item.TinhThanh,
                item.QuanHuyen,
                item.PhuongXa
            })
            .FirstOrDefaultAsync();

        if (address is null)
        {
            return "Chưa có địa chỉ mặc định.";
        }

        return string.Join(", ", new[]
        {
            AddressSerializer.ExtractStreet(address.PhuongXa),
            AddressSerializer.ExtractWard(address.PhuongXa),
            address.QuanHuyen,
            address.TinhThanh
        }.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private async Task ClearDefaultAddressesAsync(string customerId, string keepAddressId)
    {
        var addresses = await _dbContext.DiaChis
            .Where(item => item.KhachHangID == customerId && item.DiaChiID != keepAddressId && item.LaMacDinh)
            .ToListAsync();

        foreach (var address in addresses)
        {
            address.LaMacDinh = false;
        }
    }

    private static string GenerateId(string prefix, int totalLength)
    {
        var suffixLength = totalLength - prefix.Length;
        return prefix + Guid.NewGuid().ToString("N")[..suffixLength];
    }

    private static string NormalizePhoneForStorage(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? "0000000000" : digits;
    }

    private static string NormalizePhoneForDisplay(string phone)
    {
        return string.IsNullOrWhiteSpace(phone) || phone == "0000000000" ? string.Empty : phone;
    }

    private static bool TryMapOrderFilter(string status, out Enums.TrangThaiHoaDon orderStatus)
    {
        switch (status?.Trim().ToLowerInvariant())
        {
            case "confirmed":
                orderStatus = Enums.TrangThaiHoaDon.DaXacNhan;
                return true;
            case "preparing":
                orderStatus = Enums.TrangThaiHoaDon.DangChuanBi;
                return true;
            case "shipping":
                orderStatus = Enums.TrangThaiHoaDon.DangGiao;
                return true;
            case "success":
                orderStatus = Enums.TrangThaiHoaDon.HoanThanh;
                return true;
            case "cancelled":
                orderStatus = Enums.TrangThaiHoaDon.DaHuy;
                return true;
            default:
                orderStatus = Enums.TrangThaiHoaDon.ChoDuyet;
                return false;
        }
    }

    private static string GetStatusKey(Enums.TrangThaiHoaDon status)
    {
        return status switch
        {
            Enums.TrangThaiHoaDon.ChoDuyet => "pending",
            Enums.TrangThaiHoaDon.DaXacNhan => "confirmed",
            Enums.TrangThaiHoaDon.DangChuanBi => "preparing",
            Enums.TrangThaiHoaDon.DangGiao => "shipping",
            Enums.TrangThaiHoaDon.HoanThanh => "success",
            Enums.TrangThaiHoaDon.DaHuy => "cancelled",
            (Enums.TrangThaiHoaDon)6 => "returned",
            (Enums.TrangThaiHoaDon)7 => "qr_pending",
            _ => "pending"
        };
    }

    private static string GetStatusLabel(Enums.TrangThaiHoaDon status)
    {
        return status switch
        {
            Enums.TrangThaiHoaDon.ChoDuyet => "Chờ xác nhận",
            Enums.TrangThaiHoaDon.DaXacNhan => "Đã xác nhận",
            Enums.TrangThaiHoaDon.DangChuanBi => "Đang chuẩn bị",
            Enums.TrangThaiHoaDon.DangGiao => "Đang giao",
            Enums.TrangThaiHoaDon.HoanThanh => "Thành công",
            Enums.TrangThaiHoaDon.DaHuy => "Đã hủy",
            (Enums.TrangThaiHoaDon)6 => "Đã đổi trả",
            (Enums.TrangThaiHoaDon)7 => "Chờ thanh toán QR",
            _ => "Chờ xác nhận"
        };
    }
}
