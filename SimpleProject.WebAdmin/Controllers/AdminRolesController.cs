using Microsoft.AspNetCore.Mvc;
using SimpleProject.Domain;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Services;
using SimpleProject.Domain.Dtos.Admin;

namespace SimpleProject.WebAdmin.Controllers;

public class AdminRolesController : Controller
{
    private readonly IAdminRoleService _adminRoleService;
    private readonly IUserAccessor _userAccessor;
    private static readonly Dictionary<string, Func<AdminRole, Controller, object?>> _columns = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Status", (data, controller) => new object?[] { (int)data.Status, data.Status.GetDisplayName() } }
    };

    public AdminRolesController(IAdminRoleService adminRoleService, IUserAccessor userAccessor)
    {
        _adminRoleService = adminRoleService;
        _userAccessor = userAccessor;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Read(GridRequest request, string? format)
    {
        if (format == "select")
        {
            var selectQuery = this.GetSelectQuery<AdminRole>(request, (query, q) => { query.AddFilter(a => a.Name!.Contains(q)); });
            selectQuery.Select = a => new AdminRole() { Id = a.Id, Name = a.Name };
            selectQuery.AddSort(a => a.Name, true);

            return (await _adminRoleService.QueryWithTotal(selectQuery)).GetSelectResult(selectQuery, a => new ListItem(a.Id.ToString(), a.Name));
        }

        request = this.AddFilters<AdminRole>(request);
        var query = request.GetQuery<AdminRole>();

        this.StoreRequest(request);
        return (await _adminRoleService.QueryWithTotal(query)).GetGridResult(request, _columns, this);
    }

    public async Task<IActionResult> Edit(int? id, bool? modal)
    {
        ViewBag.Modal = modal.GetValueOrDefault();
        var model = new AdminRoleDto() { };
        if (id > 0)
        {
            var result = await _adminRoleService.Get<AdminRole>(a => a.Id == id);
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

            model = (AdminRoleDto)result.Data;
        }

        if (modal.GetValueOrDefault())
        {
            return PartialView(model);
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Save(AdminRoleDto entity)
    {
        if (!ModelState.IsValid)
        {
            return this.ErrorJson(ModelState);
        }

        var result = await _adminRoleService.SaveAdminRole(entity);
        if (result.HasError)
        {
            return result.ToJson();
        }
        return this.SuccesJson(new { result.Data?.Id });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        return (await _adminRoleService.DeleteAdminRole(id)).ToJson();
    }
}