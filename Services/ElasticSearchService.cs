using CatalogueWebApp.Models;
using Microsoft.Extensions.Options;
using Nest;

namespace CatalogueWebApp.Services;

/// <summary>
/// Сервис для работы с ElasticSearch.
/// </summary>
public class ElasticSearchService
{
    private readonly IElasticClient _elasticClient;

    /// <summary>
    /// Конструктор сервиса ElasticSearchService.
    /// </summary>
    /// <param name="settings">Настройки подключения к ElasticSearch.</param>
    public ElasticSearchService(IOptions<ElasticSearchSettings> settings)
    {
        var uri = new Uri(settings.Value.Uri);
        var connectionSettings = new ConnectionSettings(uri);
        _elasticClient = new ElasticClient(connectionSettings);
    }

    /// <summary>
    /// Индексирует продукт в ElasticSearch.
    /// </summary>
    /// <param name="product">Продукт для индексации.</param>
    /// <returns>Задача, представляющая асинхронную операцию индексации.</returns>
    public async Task IndexProductAsync(Product product)
    {
        await _elasticClient.IndexDocumentAsync(product);
    }

    /// <summary>
    /// Выполняет поиск продуктов в ElasticSearch по заданному запросу.
    /// </summary>
    /// <param name="query">Строка поиска.</param>
    /// <returns>Результаты поиска, содержащие найденные продукты.</returns>
    public async Task<ISearchResponse<Product>> SearchAsync(string query)
    {
        return await _elasticClient.SearchAsync<Product>(s => s
            .Query(q => q
                .Match(m => m.Field(f => f.Name).Query(query))
            )
        );
    }
}