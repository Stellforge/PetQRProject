using SimpleProject.Domain.Enums;

namespace SimpleProject.Domain.Entities;

public class EntityLog : Entity
{
    public string? TableName { get; set; }
    public int TableId { get; set; }
    public LogType LogType { get; set; }
    public int? AdminUserId { get; set; }
    public string? Changes { get; set; }
    public string? ClientIP { get; set; }

    public AdminUser? AdminUser { get; set; }

    public EntityLog() : base()
    {
    }
}
