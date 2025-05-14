namespace CatalogueWebApp.Models;

/// <summary>
/// ������ ������������� ��� �������� ������.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// ������������� ������� (RequestId).
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// ����, �����������, ����� �� ���������� ������������� �������.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}