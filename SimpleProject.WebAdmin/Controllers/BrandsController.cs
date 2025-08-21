using SimpleProject.Domain;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Dtos.Admin;
using SimpleProject.Domain.Entities;
using SimpleProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace SimpleProject.WebAdmin.Controllers;

public class BrandsController : Controller
{
    private readonly IBrandService _brandService;
    private readonly IFileService _fileService;
    private readonly IUserAccessor _userAccessor;

    private static readonly Dictionary<string, Func<Brand, Controller, object?>> _columns = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Status", (data, controller) => new object?[] { (int)data.Status, data.Status.GetDisplayName() } }
    };

    public BrandsController(IBrandService brandService, IFileService fileService, IUserAccessor userAccessor)
    {
        _brandService = brandService;
        _fileService = fileService;
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
            var selectQuery = this.GetSelectQuery<Brand>(request, (query, q) => { query.AddFilter(a => a.Name!.Contains(q)); });
            selectQuery.Select = a => new Brand() { Id = a.Id, Name = a.Name };
            selectQuery.AddSort(a => a.DisplayOrder, true);

            return (await _brandService.QueryWithTotal(selectQuery)).GetSelectResult(selectQuery, a => new ListItem(a.Id.ToString(), a.Name), nullable);
        }

        request = this.AddFilters<Brand>(request);
        var query = request.GetQuery<Brand>();

        if (format == "excel")
        {
            return (await _brandService.QueryBrandForExport(query, sample)).Excel(this);
        }

        this.StoreRequest(request);
        return (await _brandService.QueryWithTotal(query)).GetGridResult(request, _columns, this);
    }

    public async Task<IActionResult> Edit(int id, bool? modal)
    {
        ViewBag.Modal = modal.GetValueOrDefault();
        var model = new BrandDto();
        if (id > 0)
        {
            var result = await _brandService.Get<Brand>(a => a.Id == id);
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

            model = (BrandDto)result.Data;
        }

        if (modal.GetValueOrDefault())
        {
            return PartialView(model);
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Save(BrandDto entity, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            return this.ErrorJson(ModelState);
        }

        if (string.IsNullOrEmpty(entity.Image))
        {
            entity.Thumbnail = null;
        }

        var imagePaths = new List<string>();
        if (imageFile != null && imageFile.Length > 0)
        {
            var resizeConfigs = AppSettings.Current.ImageSizes?.Take(2).ToList();
            var path = "upload/images/brand/" + entity.Name!.ToUrl() + "_" + DateTime.Now.Ticks + Path.GetExtension(imageFile.FileName);
            var imageResult = await _fileService.SaveImage(imageFile.OpenReadStream(), path, resizeConfigs, true);
            if (imageResult.HasError)
            {
                return imageResult.ToJson();
            }
            entity.Image = imageResult.Data?.FirstOrDefault();
            entity.Thumbnail = imageResult.Data?.Skip(1).FirstOrDefault();
            imagePaths = imageResult.Data;
        }

        var result = await _brandService.SaveBrand(entity);
        if (result.HasError)
        {
            if (imagePaths != null)
            {
                foreach (var item in imagePaths)
                {
                    await _fileService.DeleteFile(item, true);
                }
            }
            return result.ToJson();
        }
        return this.SuccesJson(new { result.Data?.Id, result.Data?.Image, result.Data?.Thumbnail, result.Data?.DisplayOrder  });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        return (await _brandService.DeleteBrand(id)).ToJson();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        return await this.UploadExcel<Brand>(file, _brandService.SaveBrandExcel);
    }
}
