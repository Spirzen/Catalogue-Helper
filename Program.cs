using CatalogueWebApp.Models;
using CatalogueWebApp.Services;
using Microsoft.AspNetCore.DataProtection;

/// <summary>
/// Точка входа приложения.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Добавляет сервисы в контейнер внедрения зависимостей.
/// </summary>
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys")) // Для Docker
    .SetApplicationName("CatalogueWebApp");

builder.Services.AddControllersWithViews();
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.Configure<ElasticSearchSettings>(builder.Configuration.GetSection("ElasticSearchSettings"));
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<ElasticSearchService>();

var app = builder.Build();

/// <summary>
/// Настраивает HTTP-конвейер запросов.
/// </summary>
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // Включает HSTS (HTTP Strict Transport Security) для повышения безопасности.
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

/// <summary>
/// Определяет маршрут по умолчанию.
/// </summary>
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

/// <summary>
/// Запускает приложение.
/// </summary>
app.Run();