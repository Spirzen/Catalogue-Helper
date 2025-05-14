namespace CatalogueWebApp.Models;

/// <summary>
/// Модель представления для страницы ошибок.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// Идентификатор запроса (RequestId).
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Флаг, указывающий, нужно ли показывать идентификатор запроса.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}