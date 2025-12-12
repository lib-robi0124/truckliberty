using Microsoft.AspNetCore.Authentication.Cookies;
using Vozila.DataAccess.DataContext;
using Vozila.Services.AutoMappers;
using Vozila.Services.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TransporterOnly", policy =>
        policy.RequireRole("Transporter"));

    options.AddPolicy("CompanyOnly", policy =>
        policy.RequireRole("Company", "Admin"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});
// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


// Configure DbContext
builder.Services.InjectDbContext(connectionString);

// Configure Repositories
builder.Services.InjectRepositories();

// Configure Services (if any)
builder.Services.InjectServices();

// Configure AutoMapper
builder.Services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<UserMappingProfile>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Custom route for transporter login
app.MapControllerRoute(
    name: "transporterLogin",
    pattern: "Transporter/Login",
    defaults: new { controller = "Transporter", action = "Login" });

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
