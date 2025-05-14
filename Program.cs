using CatalogueWebApp.Models;
using CatalogueWebApp.Services;
using Microsoft.AspNetCore.DataProtection;

/// <summary>
/// ����� ����� ����������.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// ��������� ������� � ��������� ��������� ������������.
/// </summary>
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys")) // ��� Docker
    .SetApplicationName("CatalogueWebApp");

builder.Services.AddControllersWithViews();
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.Configure<ElasticSearchSettings>(builder.Configuration.GetSection("ElasticSearchSettings"));
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<ElasticSearchService>();

var app = builder.Build();

/// <summary>
/// ����������� HTTP-�������� ��������.
/// </summary>
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // �������� HSTS (HTTP Strict Transport Security) ��� ��������� ������������.
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

/// <summary>
/// ���������� ������� �� ���������.
/// </summary>
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

/// <summary>
/// ��������� ����������.
/// </summary>
app.Run();