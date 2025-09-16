using AjeboCustomerPortal.Data;
using AjeboCustomerPortal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity (no roles)
builder.Services.AddDefaultIdentity<UserDetailes>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;       // tweak as you like
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultUI()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication()
    .AddGoogle(google =>
    {
        google.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        google.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        // optional:
        // google.CallbackPath = "/signin-google"; // default
    });

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Identity/Account/Login";
    o.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // needed for Identity UI
builder.Services.AddScoped<AjeboCustomerPortal.Services.ReceiptPdfService>();
QuestPDF.Settings.License = LicenseType.Community;


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Default route: use Apartments (plural) if your controller is ApartmentsController
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Apartments}/{action=Index}/{id?}");

// Identity UI endpoints
app.MapRazorPages();

app.Run();
