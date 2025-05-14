using CatalogueWebApp.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CatalogueWebApp.Services;

/// <summary>
/// Сервис для работы с MongoDB.
/// </summary>
public class MongoDbService
{
    private readonly IMongoCollection<Product> _products;

    /// <summary>
    /// Конструктор сервиса MongoDbService.
    /// </summary>
    /// <param name="settings">Настройки подключения к MongoDB.</param>
    public MongoDbService(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _products = database.GetCollection<Product>("products");
    }

    /// <summary>
    /// Получает все продукты из базы данных.
    /// </summary>
    /// <returns>Список всех продуктов.</returns>
    public async Task<List<Product>> GetProductsAsync()
    {
        return await _products.Find(_ => true).ToListAsync();
    }

    /// <summary>
    /// Добавляет новый продукт в базу данных.
    /// </summary>
    /// <param name="product">Продукт для добавления.</param>
    public async Task AddProductAsync(Product product)
    {
        await _products.InsertOneAsync(product);
    }

    /// <summary>
    /// Инициализирует базу данных, удаляя старые данные и добавляя новые.
    /// </summary>
    /// <param name="products">Список продуктов для инициализации базы данных.</param>
    public async Task InitializeDatabaseAsync(List<Product> products)
    {
        await _products.Database.DropCollectionAsync("products");
        await _products.InsertManyAsync(products);
    }
}