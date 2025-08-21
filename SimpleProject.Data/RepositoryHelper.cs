using SimpleProject.Domain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace SimpleProject.Data;

public class RepositoryHelper
{
    private static readonly Dictionary<Type, List<string>> _UpdateIgnores = [];

    public static void SetUpdateIgnore<T>(EntityEntry<T> entry) where T : Entity
    {
        if (!_UpdateIgnores.ContainsKey(typeof(T)))
        {
            _UpdateIgnores[typeof(T)] = [.. typeof(T).GetProperties().Where(a => Attribute.IsDefined(a, typeof(NoUpdateAttribute))).Select(a => a.Name)];
        }

        foreach (var item in _UpdateIgnores[typeof(T)])
        {
            entry.Property(item).IsModified = false;
        }
    }

    public static void SetUnModified<T>(EntityEntry<T> entry) where T : Entity
    {
        foreach (var item in entry.Properties)
        {
            item.IsModified = false;
        }
        entry.State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
    }
}
