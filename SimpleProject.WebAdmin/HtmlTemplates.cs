using SimpleProject.Domain;

namespace SimpleProject.WebAdmin;

public class HtmlTemplates
{
    public const string Edit = @"<div class=""btn-list"">
			<button class=""btn btn-sm btn-icon btn-primary-light grid-edit"" data-id=""{0}"">
				<i class=""ri-edit-box-line""></i>
			</button>
			<button class=""btn btn-sm btn-icon btn-danger-light grid-delete"" data-id=""{0}"">
				<i class=""ri-delete-bin-line""></i>
			</button>
		</div>";
    public const string EditSimple = @"<div class=""btn-list"">
			<button class=""btn btn-sm btn-icon btn-primary-light grid-edit"" data-id=""{0}"">
				<i class=""ri-edit-box-line""></i>
			</button>
		</div>";
    public const string Unchecked = @"<i class=""ri-checkbox-blank-line fs-5 lh-1""></i>";
    public const string Checked = @"<i class=""ri-checkbox-line fs-5 lh-1""></i>";
    public const string Thumbnail = @"<span class=""avatar avatar-md avatar-square bg-light""><img src=""{0}"" class=""w-100 h-100"" /></span>";
    public const string NoImage = @"<span class=""avatar avatar-md avatar-square bg-light""><img src=""/assets/images/no-image.png"" class=""w-100 h-100"" /></span>";
    public static string Flags<T>(T value, string[] values) where T : Enum
    {
        var items = new List<string>();
        var type = typeof(T);
        foreach (Enum item in Enum.GetValues(type))
        {
            if (Convert.ToInt32(item) == 0)
            {
                continue;
            }

            if (value.HasFlag(item))
            {
                var index = (int)(Math.Log(Convert.ToInt32(item)) / Math.Log(2)) + 1;
                items.Add(string.Format(values[index], item.GetDisplayName()));
            }
        }
        return string.Join(", ", items);
    }
    public static readonly string[] Badge =
    [
        @"<span class=""badge bg-primary-transparent"">{0}</span>",
        @"<span class=""badge bg-primary1-transparent"">{0}</span>",
        @"<span class=""badge bg-primary2-transparent"">{0}</span>",
        @"<span class=""badge bg-primary3-transparent"">{0}</span>",
        @"<span class=""badge bg-secondary-transparent"">{0}</span>",
        @"<span class=""badge bg-info-transparent"">{0}</span>",
        @"<span class=""badge bg-success-transparent"">{0}</span>",
        @"<span class=""badge bg-light-transparent"">{0}</span>",
        @"<span class=""badge bg-dark-transparent"">{0}</span>",
        @"<span class=""badge bg-warning-transparent"">{0}</span>",
        @"<span class=""badge bg-danger-transparent"">{0}</span>"
    ];
    public static readonly string[] Status =
    [
        @"<span class=""badge bg-danger-transparent"">{0}</span>",
        @"<span class=""badge bg-primary-transparent"">{0}</span>"
    ];
    public static readonly string[] Availability =
    [
        @"<span class=""badge bg-primary-transparent"">{0}</span>",
        @"<span class=""badge bg-warning-transparent"">{0}</span>",
        @"<span class=""badge bg-danger-transparent"">{0}</span>"
    ];
    public static readonly string[] LogType =
    [
        @"<span class=""badge bg-primary-transparent"">{0}</span>",
        @"<span class=""badge bg-secondary-transparent"">{0}</span>",
        @"<span class=""badge bg-danger-transparent"">{0}</span>"
    ];
}
