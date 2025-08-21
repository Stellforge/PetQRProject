using SimpleProject.Domain;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Dtos.Admin;
using SimpleProject.Domain.Entities;
using SimpleProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SimpleProject.WebAdmin.Controllers;

public class CustomFieldsController : Controller
{
    private readonly ICustomFieldService _customFieldService;
    private readonly IUserAccessor _userAccessor;

    private static readonly Dictionary<string, Func<CustomField, Controller, object?>> _columns = new(StringComparer.OrdinalIgnoreCase)
    {
        { "FieldType", (data, controller) => new object?[] { (int)data.FieldType, data.FieldType.GetDisplayName() } },
        { "Status", (data, controller) => new object?[] { (int)data.Status, data.Status.GetDisplayName() } }
    };
    private static readonly List<SelectListItem> TableNames =
    [
        new SelectListItem { Text = "Ürün", Value = "Product" },
    ];

    public CustomFieldsController(ICustomFieldService customFieldService, IUserAccessor userAccessor)
    {
        _customFieldService = customFieldService;
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
            var selectQuery = this.GetSelectQuery<CustomField>(request, (query, q) => { query.AddFilter(a => a.Name!.Contains(q)); });
            selectQuery.Select = a => new CustomField() { Id = a.Id, Name = a.Name };
            selectQuery.AddSort(a => a.DisplayOrder, true);

            return (await _customFieldService.QueryWithTotal(selectQuery)).GetSelectResult(selectQuery, a => new ListItem(a.Id.ToString(), a.Name), nullable);
        }

        request = this.AddFilters<CustomField>(request);
        var query = request.GetQuery<CustomField>();
        
        if (format == "excel")
        {
            return (await _customFieldService.QueryCustomFieldForExport(query, sample)).Excel(this);
        }

        this.StoreRequest(request);
        return (await _customFieldService.QueryWithTotal(query)).GetGridResult(request, _columns, this);
    }
   
    public async Task<IActionResult> Edit(int id, bool? modal)
    {
        ViewBag.Modal = modal.GetValueOrDefault();
        ViewBag.TableNames = TableNames;

        var model = new CustomFieldDto();
        if (id > 0)
        {
            var result = await _customFieldService.Get<CustomField>(a => a.Id == id);
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
                ModelState.AddModelError("", "Kayýt bulunamadý");
                if (modal.GetValueOrDefault())
                {
                    return PartialView();
                }
                return View();
            }

            model = (CustomFieldDto)result.Data;
        }

        if (modal.GetValueOrDefault())
        {
            return PartialView(model);
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Save(CustomFieldDto entity)
    {
        if (!ModelState.IsValid)
        {
            return this.ErrorJson(ModelState);
        }

        var result = await _customFieldService.SaveCustomField(entity);
        if (result.HasError)
        {
            return result.ToJson();
        }

        return this.SuccesJson(new { result.Data?.Id, result.Data?.DisplayOrder });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        return (await _customFieldService.DeleteCustomField(id)).ToJson();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        return await this.UploadExcel<CustomField>(file, _customFieldService.SaveCustomFieldExcel);
    }
}