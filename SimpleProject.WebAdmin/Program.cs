using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.FileProviders;
using SimpleProject.Domain;
using SimpleProject.Services;
using SimpleProject.Services.Abstract;
using SimpleProject.Services.Concrete;
using SimpleProject.WebAdmin;
using SimpleProject.WebAdmin.Providers;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

//user accessor
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IUserAccessor, UserAccessor>();
//validation adapter
builder.Services.AddSingleton<IValidationAttributeAdapterProvider, ValidationAdapterProvider>();
//default services (IScopedService, ISingletonService, AppDbContext, LogDbContext, IUnitOfWork, IRepository<,>)
StartUp.ConfigureServices(builder.Services, builder.Configuration);
//mvc
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(typeof(SessionFilterAttribute));
    options.Filters.Add(typeof(DateConvertFilterAttribute));
    options.Filters.Add(new ResponseCacheAttribute()
    {
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true
    });
}).AddViewOptions(options =>
{
    options.HtmlHelperOptions.FormInputRenderMode = FormInputRenderMode.AlwaysUseCurrentCulture;
});

builder.Services.AddScoped<IAppUserService, AppUserService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IPetImageService, PetImageService>();
builder.Services.AddScoped<ICollarService, CollarService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IQrResolver, QrResolver>();
builder.Services.AddScoped<ILostReportService, LostReportService>();
builder.Services.AddScoped<IFoundReportService, FoundReportService>();
builder.Services.AddScoped<IScanEventService, ScanEventService>();
builder.Services.AddScoped<INotificationService, NotificationService>();


var app = builder.Build();

//localization
var supportedCultures = new List<CultureInfo>() { new("tr-TR") };
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(supportedCultures.First()),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
};
var cookieRequestCultureProvider = localizationOptions.RequestCultureProviders.OfType<CookieRequestCultureProvider>().First();
cookieRequestCultureProvider.CookieName = Consts.CookieLang;
localizationOptions.RequestCultureProviders.Clear();
localizationOptions.RequestCultureProviders.Add(cookieRequestCultureProvider);

app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

if (!string.IsNullOrEmpty(AppSettings.Current.UploadPath))
{
    app.UseStaticFiles(new StaticFileOptions()
    {
        FileProvider = new PhysicalFileProvider(AppSettings.Current.UploadPath),
        RequestPath = "/upload"
    });
}

app.UseRouting();

app.UseSession();
app.UseCookiePolicy(new CookiePolicyOptions()
{
    Secure = CookieSecurePolicy.SameAsRequest
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
