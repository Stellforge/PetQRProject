namespace SimpleProject.Domain.Entities;

public class ErrorLog : Entity
{
    public int? AdminUserId { get; set; }
    public string? ClientIP { get; set; }
    public string? RequestLink { get; set; }
    public string? ErrorMessage { get; set; }

    public AdminUser? AdminUser { get; set; }

    public ErrorLog() : base()
    {
    }
}
