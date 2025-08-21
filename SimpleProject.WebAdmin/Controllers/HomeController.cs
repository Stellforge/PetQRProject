using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleProject.Domain.Dtos.Admin;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Domain;
using SimpleProject.Services;
using System.Text.Json.Nodes;

namespace SimpleProject.WebAdmin.Controllers;

public class HomeController : Controller
{
    private readonly IUserAccessor _userAccessor;
    private readonly IAdminUserService _adminUserService;
    private readonly IFileService _fileService;
    private static readonly Dictionary<string, Func<EntityLog, Controller, object?>> _columns = new(StringComparer.OrdinalIgnoreCase)
    {
        { "LogType", (data, controller) => new object?[] { (int)data.LogType, data.LogType.GetDisplayName() } }
    };

    public HomeController(IAdminUserService adminUserService, IUserAccessor userAccessor, IFileService fileService)
    {
        _userAccessor = userAccessor;
        _adminUserService = adminUserService;
        _fileService = fileService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl)
    {
        var model = new LoginRequest()
        {
            ReturnUrl = returnUrl
        };

        return View(model);
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest request, string? token)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        if (!string.IsNullOrEmpty(request.ReturnUrl))
        {
            if (request.ReturnUrl.StartsWith("http:"))
            {
                request.ReturnUrl = "";
            }
        }

        //if (!string.IsNullOrEmpty(AppSettings.Current.RecaptchaSiteKey) && !await CheckReCaptcha(token))
        //{
        //    ModelState.AddModelError("", "Geçersiz captcha");
        //    return View(request);
        //}

        var result = await _adminUserService.Login(request);
        if (result.HasError)
        {
            ModelState.AddModelError("", string.Join(",", result.Errors));
            return View(request);
        }

        if (result.Data?.AdminRole != null)
        {
            var role = (AdminRoleDto)result.Data.AdminRole;
            if (role.Settings != null)
            {
                _userAccessor.Store("LeftMenu", role.Settings.UseDefaultMenu ? [.. LeftMenu.Default.Select(a => MemberWiseCloner<MenuItem>.Clone(a))] : role.Settings.Menus ?? []);
            }
        }

        if (string.IsNullOrEmpty(request.ReturnUrl))
        {
            return Redirect("/");
        }
        else
        {
            return Redirect(request.ReturnUrl);
        }
    }

    [AllowAnonymous]
    public IActionResult Logout()
    {
        _adminUserService.Logout();
        return Redirect("/home/login");
    }

    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        ViewBag.Success = false;
        return View();
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email, string? token)
    {
        ViewBag.Success = false;

        if (!ModelState.IsValid)
        {
            return View();
        }

        //if (!string.IsNullOrEmpty(AppSettings.Current.RecaptchaSiteKey) && !await CheckReCaptcha(token))
        //{
        //    ModelState.AddModelError("", "Geçersiz captcha");
        //    return View();
        //}

        var result = await _adminUserService.SendForgotPasswordMail(email);
        if (result.HasError)
        {
            result.AddModelError(this);
            return View();
        }

        ViewBag.Success = true;

        return View();
    }

    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(int id, string utoken)
    {
        var user = await _adminUserService.Get<AdminUser>(a => a.Id == id);
        if (user.HasError)
        {
            return user.ToView(this);
        }

        if (user.Data == null || !(user.Data.Id + "-" + AppSettings.Current.StoreKey).Md5().Equals(utoken, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("", "Geçersiz link");
            return View();
        }

        return View(new ResetPasswordRequest()
        {
            UToken = utoken,
            UserId = id
        });
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, string? token)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        //if (!string.IsNullOrEmpty(AppSettings.Current.RecaptchaSiteKey) && !await CheckReCaptcha(token))
        //{
        //    ModelState.AddModelError("", "Geçersiz captcha");
        //    return View(request);
        //}

        var result = await _adminUserService.ResetAdminUserPassword(request);
        if (result.HasError)
        {
            result.AddModelError(this);
            return View(request);
        }

        await _adminUserService.Logout();
        return Redirect("/login");
    }

    public async Task<IActionResult> Logs(GridRequest request)
    {
        request = this.AddFilters<EntityLog>(request);
        var query = request.GetQuery<EntityLog>();
        return (await _adminUserService.QueryWithTotal(query)).GetGridResult(request, _columns, this);
    }

    [HttpPost]
    public async Task<IActionResult> SaveImage(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return this.ErrorJson("Dosya boþ olamaz");
        }
        return (await _fileService.SaveFile(file.OpenReadStream(), "upload/contents/" + Path.GetFileNameWithoutExtension(file.FileName) + "_" + DateTime.Now.Ticks + Path.GetExtension(file.FileName), true)).ToJson();
    }

    [HttpPost]
    public async Task<IActionResult> GetFiles(string path)
    {
        return (await _fileService.GetFiles(path)).ToJson();
    }

    [HttpPost]
    public async Task<IActionResult> DeleteFile(string path)
    {
        return (await _fileService.DeleteFile(path, true)).ToJson();
    }

    [HttpPost]
    public async Task<IActionResult> CreateFolder(string path, string name)
    {
        return (await _fileService.CreateFolder(path, name)).ToJson();
    }

    [HttpPost]
    public async Task<IActionResult> UploadFile(string path, IFormFile? imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
        {
            return this.ErrorJson("Dosya boþ olamaz");
        }
        if (string.IsNullOrEmpty(path))
        {
            path = "/upload";
        }
        if (!path.StartsWith("/upload"))
        {
            path = "/upload/" + path.TrimStart('/');
        }
        return (await _fileService.SaveFile(imageFile.OpenReadStream(), path.TrimEnd('/') + "/" + imageFile.FileName, true)).ToJson();
    }

    [HttpPost]
    public async Task<IActionResult> RenameFile(string path, string name)
    {
        return (await _fileService.RenameFile(path, name)).ToJson();
    }

    public IActionResult Error()
    {
        return View();
    }

    private async Task<bool> CheckReCaptcha(string? token)
    {
        try
        {
            var postData = new List<KeyValuePair<string, string?>>()
            {
                //new("secret", AppSettings.Current.RecaptchaSecret),
                new("remoteip", HttpContext.Connection?.RemoteIpAddress?.ToString()),
                new("response", token)
            };

            var client = new HttpClient();
            var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", new FormUrlEncodedContent(postData));

            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(content))
            {
                var result = JsonNode.Parse(content);
                if (result != null)
                {
                    var json = result.AsObject();
                    if (json.TryGetPropertyValue("success", out JsonNode? node))
                    {
                        return node!.GetValue<bool>();
                    }
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            var _logService = HttpContext.RequestServices.GetService<ILogService>();
            if (_logService != null)
            {
                await _logService.LogException(ex);
            }
        }
        return false;
    }
}
