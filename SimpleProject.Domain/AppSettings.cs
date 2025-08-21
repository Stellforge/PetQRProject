using SimpleProject.Domain.Dtos;

namespace SimpleProject.Domain;

public class AppSettings
{
    public bool IsTest { get; set; }
    public string? ConnectionString { get; set; }
    public string? LogConnectionString { get; set; }
    public string? StoreKey { get; set; }
    public int MaxExcelRowCount { get; set; }
    public string? Domain { get; set; }
    public string? AdminDomain { get; set; }
    public string? UploadPath { get; set; }
    public List<ResizeConfig>? ImageSizes { get; set; }
    public static AppSettings Current { get; set; } = new AppSettings();
}
