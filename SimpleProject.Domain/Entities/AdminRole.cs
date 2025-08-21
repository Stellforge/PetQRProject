using SimpleProject.Domain.Enums;

namespace SimpleProject.Domain.Entities;

public class AdminRole : Entity
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Settings { get; set; }
    public Status Status { get; set; }

    public List<AdminUser>? AdminUsers { get; set; }

    public AdminRole()
    {
        Status = Status.ACTIVE;
    }
}
