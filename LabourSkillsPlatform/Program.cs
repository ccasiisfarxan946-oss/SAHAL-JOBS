using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using LabourSkillsPlatform.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Data Tier: EF Core + SQL Server (Chapter 3, 3.3) ----------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ---------- Identity + RBAC (Chapter 3, 3.2.1 Design) ----------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ---------- Application Tier: Services (Chapter 3, 3.4) ----------
builder.Services.AddScoped<IJobMatchingService, JobMatchingService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

// ---------- Authorization policies for area-based RBAC ----------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Roles.Admin, policy => policy.RequireRole(Roles.Admin));
    options.AddPolicy(Roles.Employer, policy => policy.RequireRole(Roles.Employer));
    options.AddPolicy(Roles.Labourer, policy => policy.RequireRole(Roles.Labourer));
});

// ---------- Presentation Tier: Razor Pages with Areas per role ----------
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Admin", "/", Roles.Admin);
    options.Conventions.AuthorizeAreaFolder("Employer", "/", Roles.Employer);
    options.Conventions.AuthorizeAreaFolder("Labourer", "/", Roles.Labourer);
});

var app = builder.Build();

// ---------- Seed roles, admin account and reference data ----------
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
