using SimpleProject.Domain.Dtos.Admin;

namespace SimpleProject.Domain.Dtos.Settings;

public class RoleSettings
{
    public bool UseDefaultMenu { get; set; }
    public List<MenuItem>? Menus { get; set; }
}
