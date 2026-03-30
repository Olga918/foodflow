using FoodFlow.Data;
using FoodFlow.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Настройка базы данных (MySQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySQL(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 2. Identity и роли
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Home/AccessDenied";
});

// 3. Сервисы MVC и Сессии
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(2);
});

var app = builder.Build();

// --- ПРАВИЛЬНЫЙ ЗАПУСК СИДЕРА ---
// Запускаем заполнение БД в фоновом потоке, чтобы порт открылся НЕМЕДЛЕННО
_ = Task.Run(async () =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        // Передаем провайдер сервисов внутрь
        await DbSeeder.SeedAsync(scope.ServiceProvider);
        Console.WriteLine("--> Database Seeding completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> Error during Database Seeding: {ex.Message}");
    }
});
// --------------------------------

// 4. HTTP Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // ВАЖНО: При работе за Cloudflare/Nginx HSTS обычно настраивается на стороне прокси
    app.UseHsts();
}

// Если Cloudflare настроен на "Always use HTTPS", эту строку можно закомментировать, 
// чтобы избежать цикличных редиректов (Redirect Loop)
// app.UseHttpsRedirection(); 

app.UseStaticFiles(); // Стандартный метод для статики
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
