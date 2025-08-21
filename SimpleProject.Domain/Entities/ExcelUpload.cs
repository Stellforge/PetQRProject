namespace SimpleProject.Domain.Entities;

public class ExcelUpload : Entity
{
    public int AdminUserId { get; set; }
    public string? Name { get; set; }
    public string? UploadType { get; set; }
    public string? InterfaceName { get; set; }
    public string? Method { get; set; }
    public string? ExtraParameter { get; set; }
    public int Total { get; set; }
    public int Success { get; set; }
    public int Fail { get; set; }
    public string? FilePath { get; set; }
    public string? ErrorFilePath { get; set; }
    public string? ErrorMessage { get; set; }
    public bool Completed { get; set; }

    public AdminUser? AdminUser { get; set; }

    public ExcelUpload() : base()
    {
    }
}
