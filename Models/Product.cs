using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace CatalogueWebApp.Models;

/// <summary>
/// Модель продукта для хранения в MongoDB.
/// </summary>
public class Product
{
    /// <summary>
    /// Уникальный идентификатор продукта (ObjectId в MongoDB).
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    /// <summary>
    /// Название продукта.
    /// </summary>
    [BsonElement("name")]
    public string Name { get; set; }

    /// <summary>
    /// Описание продукта.
    /// </summary>
    [BsonElement("description")]
    public string Description { get; set; }

    /// <summary>
    /// Цена продукта.
    /// </summary>
    [BsonElement("price")]
    public decimal Price { get; set; }

    /// <summary>
    /// Категория продукта.
    /// </summary>
    [BsonElement("category")]
    public string Category { get; set; }

    /// <summary>
    /// Теги продукта.
    /// </summary>
    [BsonElement("tags")]
    public List<string> Tags { get; set; }

    /// <summary>
    /// Дата создания продукта.
    /// </summary>
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; }
}