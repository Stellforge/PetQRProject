using SimpleProject.Domain;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Domain.Dtos.Admin;
using SimpleProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace SimpleProject.WebAdmin.Controllers;

public class CustomFieldOptionsController : Controller
{
    private readonly ICustomFieldOptionService _customFieldOptionService;
    private readonly IUserAccessor _userAccessor;

    private static readonly Dictionary<string, Func<CustomFieldOption, Controller, object?>> _columns = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Status", (data, controller) => new object?[] { (int)data.Status, data.Status.GetDisplayName() } }
    };

    public CustomFieldOptionsController(ICustomFieldOptionService customFieldOptionService, IUserAccessor userAccessor)
    {
        _customFieldOptionService = customFieldOptionService;
        _userAccessor = userAccessor;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Read(GridRequest request, string? format, bool? sample, bool? nullable)
    {
        if (format == "select")
        {
            var selectQuery = this.GetSelectQuery<CustomFieldOption>(request, (query, q) => { query.AddFilter(a => a.Name!.Contains(q)); });
            selectQuery.Select = a => new CustomFieldOption() { Id = a.Id, Name = a.Name };
            selectQuery.AddSort(a => a.DisplayOrder, true);

            return (await _customFieldOptionService.QueryWithTotal(selectQuery)).GetSelectResult(selectQuery, a => new ListItem(a.Id.ToString(), a.Name), nullable);
        }

        request = this.AddFilters<CustomFieldOption>(request);
        var query = request.GetQuery<CustomFieldOption>();

        if (format == "excel")
        {
            return (await _customFieldOptionService.QueryCustomFieldOptionForExport(query, sample)).Excel(this);
        }

        this.StoreRequest(request);
        return (await _customFieldOptionService.QueryWithTotal(query)).GetGridResult(request, _columns, this);
    }

    public async Task<IActionResult> Edit(int id, int? customFieldId, bool? modal)
    {
        ViewBag.Modal = modal.GetValueOrDefault();
        var model = new CustomFieldOptionDto();
        if (id > 0)
        {
            var result = await _customFieldOptionService.Get<CustomFieldOption>(a => a.Id == id, a => new CustomFieldOption()
            {
                CustomField = new CustomField()
                {
                    Id = a.CustomField!.Id,
                    Name = a.CustomField!.Name,
                }
            }, true);

            if (result.HasError)
            {
                if (modal.GetValueOrDefault())
                {
                    return result.ToPartialView(this);
                }
                return result.ToView(this);
            }

            if (result.Data == null)
            {
                ModelState.AddModelError("", "Kayıt bulunamadı");
                if (modal.GetValueOrDefault())
                {
                    return PartialView();
                }
                return View();
            }
            model = (CustomFieldOptionDto)result.Data;
        }
        else if (customFieldId.HasValue)
        {
            var result = await _customFieldOptionService.Get<CustomField>(a => a.Id == customFieldId, a => new CustomField()
            {
                Id = a.Id,
                Name = a.Name
            });
            if (result.Data != null)
            {
                model.CustomFieldId = result.Data.Id;
                model.CustomFieldName = result.Data.Name;
            }
        }

        if (modal.GetValueOrDefault())
        {
            return PartialView(model);
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Save(CustomFieldOptionDto entity)
    {
        if (!ModelState.IsValid)
        {
            return this.ErrorJson(ModelState);
        }

        var result = await _customFieldOptionService.SaveCustomFieldOption(entity);
        if (result.HasError)
        {
            return result.ToJson();
        }

        return this.SuccesJson(new { result.Data?.Id, result.Data?.DisplayOrder });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        return (await _customFieldOptionService.DeleteCustomFieldOption(id)).ToJson();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        return await this.UploadExcel<CustomFieldOption>(file, _customFieldOptionService.SaveCustomFieldOptionExcel);
    }
}