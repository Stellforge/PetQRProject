using SimpleProject.Domain.Dtos.Admin;
using System.Reflection;

namespace SimpleProject.WebAdmin;

public class LeftMenu
{
    public static List<MenuItem> Default => new()
    {
        new MenuItem(){ Name = "Dashboard", Icon = "ri-home-line", Title = "Dashboard", LangKey = "Dashboard", Url = "/" },

        new MenuItem(){ Name = "SalesTitle", Title = "Sipariş", LangKey = "Sales", IsHeader = true },
        new MenuItem(){ Name = "SalesOrder", Icon = "ri-store-3-line", Title = "Siparişler", LangKey = "Sales", Url = "/salesorders" },
        new MenuItem(){ Name = "NewSalesOrder", Icon = "ri-shopping-cart-line", Title = "Yeni", LangKey = "NewSalesOrder", Url = "/salesorders/new" },

        new MenuItem(){ Name = "CatalogTitle", Title = "Catalog", LangKey = "Katalog", IsHeader = true },
        new MenuItem(){ Name = "Brand", Icon = "ri-puzzle-line", Title = "Marka", LangKey = "Brand", Url = "/brands" },

        new MenuItem(){ Name = "CustomFieldTitle", Icon = "ri-input-field", Title = "Özel alan", LangKey = "CustomField", Url = "javascript:;" },
        new MenuItem(){ Name = "CustomField", Icon = "ri-list-indefinite", Title = "Özel alan", LangKey = "CustomField", Url = "/customfields", Level = 1 },
        new MenuItem(){ Name = "CustomFieldOption", Icon = "ri-list-view", Title = "Özel alan seçenek", LangKey = "CustomFieldOption", Url = "/customfieldoptions", Level = 1 },

        new MenuItem(){ Name = "SystemTitle", Icon = "ri-settings-4-line", Title = "Sistem", LangKey = "System", Url = "javascript:;" },
        new MenuItem(){ Name = "AdminUser", Icon = "ri-user-line", Title = "Kullanıcı", LangKey = "AdminUser", Url = "/adminusers", Level = 1 },
        new MenuItem(){ Name = "AdminRole", Icon = "ri-user-settings-line", Title = "Rol", LangKey = "AdminRole", Url = "/adminroles", Level = 1 }
    };

    public static MenuItem? SetSelected(List<MenuItem> items, string url)
    {
        items.ForEach(a => a.Selected = false);
        var item = items.Where(a => !a.IsHeader && url.StartsWith(a.Url ?? "", StringComparison.OrdinalIgnoreCase)).OrderByDescending(a => (a.Url ?? "").Length).FirstOrDefault();
        item ??= items.FirstOrDefault(a => !a.IsHeader);
        if (item != null)
        {
            var index = items.FindIndex(a => a.Name == item.Name);
            var level = item.Level;
            for (int i = index; i >= 0; i--)
            {
                var element = items.ElementAt(i);
                if (!element.IsHeader && (element.Name == item.Name || element.Level < level))
                {
                    element.Selected = true;
                    level = element.Level;
                }
            }
        }
        return item;
    }

    public static List<MenuItem> BreadCrumbs(List<MenuItem> items)
    {
        return [.. items.Where(a => a.Selected)];
    }

    public static bool IsHeaderItem(MenuItem menu)
    {
        return menu.IsHeader || string.IsNullOrEmpty(menu.Url) || menu.Url == "javascript:;";
    }
    public static bool IsCustomItem(MenuItem menu)
    {
        return (menu.Name ?? "").StartsWith("Custom", StringComparison.OrdinalIgnoreCase);
    }
}
