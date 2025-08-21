using SimpleProject.Domain.Enums;

namespace SimpleProject.Domain.Entities;
public class AdminUser : Entity
{
    public int AdminRoleId { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Email { get; set; }
    public Status Status { get; set; }

    public AdminRole? AdminRole { get; set; }
    public List<ErrorLog>? ErrorLogs { get; set; }
    public List<EntityLog>? EntityLogs { get; set; }

    public AdminUser() : base()
    {
        Status = Status.ACTIVE;
    }
}
