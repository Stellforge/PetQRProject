using SimpleProject.Domain.Enums;

namespace SimpleProject.Domain.Entities;

public class Brand : Entity
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Image { get; set; }
    public string? Thumbnail { get; set; }
    public int DisplayOrder { get; set; }
    public Status Status { get; set; }

    public Brand() : base()
    {
        Status = Status.ACTIVE;
    }
}