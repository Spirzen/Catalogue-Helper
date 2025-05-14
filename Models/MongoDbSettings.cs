namespace CatalogueWebApp.Models;

/// <summary>
/// Настройки для подключения к MongoDB.
/// </summary>
public class MongoDbSettings
{
    /// <summary>
    /// Строка подключения к MongoDB.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Имя базы данных MongoDB.
    /// </summary>
    public string DatabaseName { get; set; }
}