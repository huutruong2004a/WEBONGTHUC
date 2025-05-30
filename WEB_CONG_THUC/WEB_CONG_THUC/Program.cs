using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using WEB_CONG_THUC.Data;
using WEB_CONG_THUC.Extensions.CollectionExtensions;
using WEB_CONG_THUC.Models;
using WEB_CONG_THUC.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
.AddDefaultTokenProviders()
.AddDefaultUI()
.AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
// Thêm cấu hình phân quyền
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireCustomerRole",
        policy => policy.RequireRole(SD.Role_Customer));
    options.AddPolicy("RequireAdminRole",
        policy => policy.RequireRole(SD.Role_Admin));
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

/*builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();*/
builder.Services.AddControllersWithViews();

// Thêm đăng ký cho IRecipeRepository và RecipeRepository
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IVideoRepository, VideoRepository>();
builder.Services.AddScoped<WEB_CONG_THUC.Services.GeminiFoodAssistantService>();
builder.Services.AddLogging();
builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});
builder.Services.CategoryServices();

builder.Services.BlogServices();

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = int.MaxValue; // hoặc một giá trị cụ thể như 100_000_000
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = int.MaxValue; // hoặc một giá trị cụ thể
});

builder.Services.Configure<FormOptions>(x =>
{
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartBodyLengthLimit = int.MaxValue;
    x.MultipartHeadersLengthLimit = int.MaxValue;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Đảm bảo gọi UseStaticFiles trước UseRouting
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.MapStaticAssets();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

// Thêm vào cuối file Program.cs, trước app.Run()
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Tạo role Admin nếu chưa tồn tại
    if (!await roleManager.RoleExistsAsync(SD.Role_Admin))
    {
        await roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
    }

    // Tạo tài khoản Admin mặc định nếu chưa tồn tại
    var adminUser = await userManager.FindByEmailAsync(SD.AdminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = SD.AdminEmail,
            Email = SD.AdminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin@123"); // Mật khẩu mặc định
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, SD.Role_Admin);
        }
    }
}

app.Run();