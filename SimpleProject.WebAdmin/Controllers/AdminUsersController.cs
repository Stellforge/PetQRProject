using SimpleProject.Domain;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Domain.Dtos.Admin;
using SimpleProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace SimpleProject.WebAdmin.Controllers;

public class AdminUsersController : Controller
{
    private readonly IAdminUserService _adminUserService;
    private readonly IUserAccessor _userAccessor;
    private static readonly Dictionary<string, Func<AdminUser, Controller, object?>> _columns = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Status", (data, controller) => new object?[] { (int)data.Status, data.Status.GetDisplayName() } }
    };

    public AdminUsersController(IAdminUserService adminUserService, IUserAccessor userAccessor)
    {
        _adminUserService = adminUserService;
        _userAccessor = userAccessor;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Read(GridRequest request, string? format, bool? sample)
    {
        if (format == "select")
        {
            var selectQuery = this.GetSelectQuery<AdminUser>(request, (query, q) => { query.AddFilter(a => a.Name!.Contains(q)); });
            selectQuery.Select = a => new AdminUser() { Id = a.Id, Name = a.Name };
            selectQuery.AddSort(a => a.Name, true);

            return (await _adminUserService.QueryWithTotal(selectQuery)).GetSelectResult(selectQuery, a => new ListItem(a.Id.ToString(), a.Name));
        }

        request = this.AddFilters<AdminUser>(request);
        var query = request.GetQuery<AdminUser>();

        if (format == "excel")
        {
            return (await _adminUserService.QueryAdminUserForExport(query, sample)).Excel(this);
        }

        this.StoreRequest(request);
        return (await _adminUserService.QueryWithTotal(query)).GetGridResult(request, _columns, this);
    }

    public async Task<IActionResult> Edit(int? id, int? adminRoleId, bool? modal)
    {
        ViewBag.Modal = modal.GetValueOrDefault();
        var model = new AdminUserDto();
        if (id > 0)
        {
            var result = await _adminUserService.Get<AdminUser>(a => a.Id == id, a => new AdminUser() 
            {
                AdminRole = new AdminRole()
                {
                    Id = a.AdminRole!.Id,
                    Name = a.AdminRole!.Name
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

            model = (AdminUserDto)result.Data;
        }
        else if (adminRoleId.HasValue)
        {
            var result = await _adminUserService.Get<AdminRole>(a => a.Id == adminRoleId, a => new AdminRole()
            {
                Id = a.Id,
                Name = a.Name
            });

            if (result.HasError)
            {
                if (modal.GetValueOrDefault())
                {
                    return result.ToPartialView(this);
                }
                return result.ToView(this);
            }

            if (result.Data != null)
            {
                model.AdminRoleId = result.Data.Id;
                model.AdminRoleName = result.Data.Name;
            }
        }

        if (modal.GetValueOrDefault())
        {
            return PartialView(model);
        }
        return View(model);
    }
    
    [HttpPost]
    public async Task<IActionResult> Save(AdminUserDto entity)
    {
        if (!ModelState.IsValid)
        {
            return this.ErrorJson(ModelState);
        }

        var result = await _adminUserService.SaveAdminUser(entity);
        if (result.HasError)
        {
            return result.ToJson();
        }
        return this.SuccesJson(new { result.Data?.Id });
    }
    
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        return (await _adminUserService.DeleteAdminUser(id)).ToJson();
    }
    
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        return await this.UploadExcel<AdminUser>(file, _adminUserService.SaveAdminUserExcel);
    }
}
