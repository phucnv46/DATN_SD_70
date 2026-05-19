<<<<<<< HEAD
﻿using PayOS; // ĐÃ SỬA: Namespace của bản mới nhất 2.1.0
=======
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

<<<<<<< HEAD
// =======================================================
// KHỞI TẠO CỔNG THANH TOÁN PAYOS V2.1.0
// =======================================================
IConfiguration configuration = builder.Configuration;
PayOSClient payOSClient = new PayOSClient(
    configuration["PayOS:ClientId"] ?? "",
    configuration["PayOS:ApiKey"] ?? "",
    configuration["PayOS:ChecksumKey"] ?? ""
);
builder.Services.AddSingleton(payOSClient);
// =======================================================

=======
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
builder.Services.AddDbContext<DATN_70.Data.AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<DATN_70.Data.SqlConnectionFactory>();
<<<<<<< HEAD
=======
builder.Services.AddScoped<DATN_70.Data.StorefrontDataSeeder>();
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
builder.Services.AddScoped<DATN_70.Services.IStoreRepository, DATN_70.Services.StoreRepository>();

builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DATN_70.Data.AppDbContext>();
    await dbContext.Database.MigrateAsync();

<<<<<<< HEAD
    
=======
    var seeder = scope.ServiceProvider.GetRequiredService<DATN_70.Data.StorefrontDataSeeder>();
    await seeder.SeedAsync();
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
}
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

<<<<<<< HEAD
app.Run();
=======
app.Run();
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
