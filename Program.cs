using SIMS_Project.Data;
using SIMS_Project.Interface;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký Repository (Để Controller dùng được _userRepo)
// Mỗi lần Controller cần IUserRepository, nó sẽ tạo ra một LoginRepository mới
builder.Services.AddScoped<IUserRepository, LoginRepository>();

// 2. Cấu hình Authentication (Đăng nhập bằng Cookie)
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.Name = "SIMS_Cookie"; // Tên cookie lưu trên trình duyệt
        options.LoginPath = "/Login/Login";  // Nếu chưa đăng nhập thì chuyển hướng về đây
        options.AccessDeniedPath = "/Login/AccessDenied"; // Nếu không có quyền thì chuyển về đây
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Cookie hết hạn sau 30 phút
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 3. Kích hoạt Authentication (Xác thực - Bạn là ai?)
// Phải đặt TRƯỚC Authorization
app.UseAuthentication();

// 4. Kích hoạt Authorization (Phân quyền - Bạn được làm gì?)
app.UseAuthorization();


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();