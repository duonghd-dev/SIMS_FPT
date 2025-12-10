using SIMS_FPT.Business.Interfaces; 
using SIMS_FPT.Business.Services;   
using SIMS_FPT.Data.Interfaces;
using SIMS_FPT.Data.Repositories;
using SIMS_FPT.Services;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------
// 1. Đăng ký Repository
// Mỗi lần Controller cần IUserRepository -> tạo UsersRepository
// ------------------------------------------------------
builder.Services.AddScoped<IUserRepository, UsersRepository>();


// Student Repository
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<StudentService>();

// Subject Repository
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();


// Teacher Repository
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<TeacherService>();

// Holiday Repository
// builder.Services.AddScoped<IHolidayRepository, HolidayRepository>();

// Fees Structure Repository
// builder.Services.AddScoped<IFeesStructureRepository, FeesStructureRepository>();


// // Department Repository
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();

<<<<<<< HEAD

=======
// Expense, Fee, Salary Repositories
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IFeeRepository, FeeRepository>();
builder.Services.AddScoped<ISalaryRepository, SalaryRepository>();

// Assignment & Submission Repositories
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();
builder.Services.AddScoped<IGradingService, GradingService>();
>>>>>>> ede8857e55285dbd2b9da95219eff4bc0035a489











// ------------------------------------------------------
// 2. Cấu hình Authentication (Đăng nhập bằng Cookie)
// ------------------------------------------------------
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.Name = "SIMS_Cookie";                 // Tên cookie lưu trên trình duyệt
        options.LoginPath = "/Users/Login";                  // Đúng controller: Users/Login
        options.AccessDeniedPath = "/Users/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);   // Hết hạn 30 phút
    });

// Load MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ------------------------------------------------------
// Middleware pipeline
// ------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ⚠️ Authentication phải đặt trước Authorization
app.UseAuthentication();
app.UseAuthorization();

// ------------------------------------------------------
// Định nghĩa route cho Areas
// ------------------------------------------------------
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Dashboard}/{id?}");



// ------------------------------------------------------
// Route mặc định
// ------------------------------------------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
