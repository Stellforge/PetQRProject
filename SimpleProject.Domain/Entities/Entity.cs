using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SimpleProject.Domain.Entities;

public class Entity
{
    [Key]
    public int Id { get; set; }

    public bool Deleted { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreateDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UpdateDate { get; set; }

    public Entity()
    {
        Deleted = false;
        CreateDate = DateTime.UtcNow;
        UpdateDate = DateTime.UtcNow;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Id.Equals(((Entity)obj).Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
