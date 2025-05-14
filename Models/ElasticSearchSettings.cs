namespace CatalogueWebApp.Models;

/// <summary>
/// Настройки для подключения к ElasticSearch.
/// </summary>
public class ElasticSearchSettings
{
    /// <summary>
    /// URI для подключения к ElasticSearch.
    /// </summary>
    public string Uri { get; set; }
}