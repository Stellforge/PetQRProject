using SimpleProject.Data;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Domain.Enums;
using SimpleProject.Domain;
using Microsoft.Extensions.DependencyInjection;
using SimpleProject.Domain.Dtos.Admin;
using System.Data;

namespace SimpleProject.Services;

public interface IAdminUserService : IServiceBase, IScopedService
{
    Task<Result<AdminUser>> SaveAdminUser(AdminUserDto data);
    Task<Result> DeleteAdminUser(int id);
    internal Task DeleteAdminUser(AdminUser entity);
    Task<Result<IEnumerable<AdminUserDto>>> QueryAdminUserForExport(Query<AdminUser> query, bool? isSample = null);
    Task<Result> SaveAdminUserExcel(ExcelUploadDto data);

    Task<Result<AdminUser>> Login(LoginRequest request);
    Task<Result<AdminUser>> Login(int id);
    Task<Result> Logout();

    Task<Result> SendForgotPasswordMail(string email);
    Task<Result<AdminUser>> ResetAdminUserPassword(ResetPasswordRequest request);
}

public class AdminUserService : ServiceBase, IAdminUserService
{
    private IAdminUserService Self => this;
    private readonly IExcelService _excelService;
    private readonly IRepository<AdminUser> _repositoryAdminUser;
    private readonly IRepository<AdminRole> _repositoryAdminRole;
    private readonly IRepository<ExcelUpload> _repositoryExcelUpload;

    public AdminUserService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _excelService = _serviceProvider.GetRequiredService<IExcelService>();
        _repositoryAdminUser = _serviceProvider.GetRequiredService<IRepository<AdminUser>>();
        _repositoryAdminRole = _serviceProvider.GetRequiredService<IRepository<AdminRole>>();
        _repositoryExcelUpload = _serviceProvider.GetRequiredService<IRepository<ExcelUpload>>();
    }

    public async Task<Result<AdminUser>> SaveAdminUser(AdminUserDto data)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            var validationResult = _validation.Validate(data);
            if (validationResult.HasError)
            {
                return new Result<AdminUser>(validationResult);
            }

            var entity = (AdminUser)data;
            var oldEntity = default(AdminUser);

            if (entity.Id > 0)
            {
                oldEntity = await _repositoryAdminUser.Get(a => a.Id == entity.Id) ?? throw new BusException("Kayıt bulunamadı");

                entity.CreateDate = oldEntity.CreateDate;
                entity.UpdateDate = oldEntity.UpdateDate;
            }

            if (oldEntity == null || oldEntity.Password != entity.Password)
            {
                entity.Password = entity.Password?.SHA1();
            }

            if (oldEntity == null || oldEntity.AdminRoleId != entity.AdminRoleId)
            {
                var exists = await _repositoryAdminRole.Any(a => a.Id == entity.AdminRoleId);
                if (!exists)
                {
                    throw new BusException("Rol bulunamaıd");
                }
            }

            if (oldEntity == null || oldEntity.UserName != entity.UserName)
            {
                var exists = await _repositoryAdminUser.Any(a => a.UserName == entity.UserName && a.Id != entity.Id);
                if (exists)
                {
                    throw new BusException(string.Format("{0} bu kullanıcı adı ile kayıtlı kullanıcı bulunmaktadır", entity.UserName));
                }
            }

            if (entity.Id > 0)
            {
                await _repositoryAdminUser.Update(entity, oldEntity);
            }
            else
            {
                await _repositoryAdminUser.Add(entity);
            }

            await _logService.LogEntityHistory(entity, oldEntity);

            return new Result<AdminUser>() { Data = entity };
        }
        catch (Exception ex)
        {
            if (!isTransactional)
            {
                return new Result<AdminUser>(await _logService.LogException(ex));
            }
            throw;
        }
    }
    public async Task<Result> DeleteAdminUser(int id)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            if (!isTransactional)
            {
                await _unitOfWork.BeginTransaction();
            }

            var entity = await _repositoryAdminUser.Get(a => a.Id == id, a=> new AdminUser()
            {
                Id = a.Id
            }) ?? throw new BusException("Kayıt bulunamadı");

            await Self.DeleteAdminUser(entity);

            if (!isTransactional)
            {
                await _unitOfWork.CommitTransaction();
                await _logService.WriteEntityHistories();
            }

            return new Result();
        }
        catch (Exception ex)
        {
            if (!isTransactional)
            {
                await _unitOfWork.RollbackTransaction();
                return new Result(await _logService.LogException(ex));
            }
            throw;
        }
    }
    async Task IAdminUserService.DeleteAdminUser(AdminUser entity)
    {
        await _repositoryAdminUser.Delete(entity);

        await _repositoryExcelUpload.ExecuteUpdate(a => a.AdminUserId == entity.Id, s => s
            .SetProperty(a => a.UpdateDate, DateTime.UtcNow)
            .SetProperty(a => a.Deleted, true));

        await _logService.LogDeleteHistory(entity);
    }
    public async Task<Result<IEnumerable<AdminUserDto>>> QueryAdminUserForExport(Query<AdminUser> query, bool? isSample = null)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            var columns = GetAdminUserExcelColumns();
            query.Select = columns.GetSelect<AdminUserDto, AdminUser>(a=> new AdminUser()
            {
                AdminRole = new AdminRole()
                {
                    Id = a.AdminRole!.Id,
                    Name = a.AdminRole!.Name
                }
            });
            query.Top = isSample.GetValueOrDefault() ? 5 : 0;

            var data = await _repositoryAdminUser.Query(query);
            return new Result<IEnumerable<AdminUserDto>>()
            {
                Data = [.. data.Select(a => (AdminUserDto)a)],
                Extra = new Dictionary<string, object>()
                {
                    { "Columns", columns }
                }
            };
        }
        catch (Exception ex)
        {
            if (!isTransactional)
            {
                return new Result<IEnumerable<AdminUserDto>>(await _logService.LogException(ex));
            }
            throw;
        }
    }
    public async Task<Result> SaveAdminUserExcel(ExcelUploadDto data)
    {
        try
        {
            var columns = GetAdminUserExcelColumns();
            return await _excelService.SaveExcel(data, columns, SaveAdminUser);
        }
        catch (Exception ex)
        {
            return new Result(await _logService.LogException(ex));
        }
    }

    public async Task<Result<AdminUser>> Login(LoginRequest request)
    {
        try
        {
            var pwd = request.Password?.SHA1();
            var user = await _repositoryAdminUser.Get(a => a.UserName == request.UserName && a.Password == pwd && a.Status == Status.ACTIVE, "AdminRole");
            if (user == null)
            {
                return new Result<AdminUser>("Lütfen kullanıcı adınızı ve şifrenizi kontrol ediniz");
            }

            if (_userAccessor != null)
            {
                _userAccessor.AdminUser = (AdminUserDto)user;
            }

            return new Result<AdminUser>() { Data = user };
        }
        catch (Exception ex)
        {
            return new Result<AdminUser>(await _logService.LogException(ex));
        }
    }
    public async Task<Result<AdminUser>> Login(int id)
    {
        try
        {
            var user = await _repositoryAdminUser.Get(a => a.Id == id && a.Status == Status.ACTIVE, "AdminRole");
            if (user == null)
            {
                return new Result<AdminUser>("Kayıt bulunamadı");
            }

            if (_userAccessor != null)
            {
                _userAccessor.AdminUser = (AdminUserDto)user;
            }

            return new Result<AdminUser>() { Data = user };
        }
        catch (Exception ex)
        {
            return new Result<AdminUser>(await _logService.LogException(ex));
        }
    }
    public async Task<Result> Logout()
    {
        try
        {
            _userAccessor?.Clear();
            return new Result();
        }
        catch (Exception ex)
        {
            return new Result(await _logService.LogException(ex));
        }
    }

    public async Task<Result> SendForgotPasswordMail(string email)
    {
        try
        {
            //var user = await _repositoryAdminUser.Get(a => a.Email == email && a.Status == Status.ACTIVE) ?? throw new BusException("Kayıt bulunamadı");

            //var contentData = await _repositoryContentData.Get(a => a.Content!.ContentType == ContentType.EMAIL && a.FilterValue == Consts.ContentTypes.AdminForgotPassword && a.LanguageId == languageId && a.Status == Status.ACTIVE);
            //if (contentData != null)
            //{
            //    // Create email message
            //    var mailMessage = new MailMessage()
            //    {
            //        Subject = contentData.Title,
            //        IsBodyHtml = true,
            //        Body = RepleceContentString(contentData.Data, user)
            //    };
            //    mailMessage.To.Add(user.Email!);

            //    // Send email
            //    return await emailProvider.Send(mailMessage);

            //}
            return new Result();
        }
        catch (Exception ex)
        {
            return new Result(await _logService.LogException(ex));
        }
    }
    public async Task<Result<AdminUser>> ResetAdminUserPassword(ResetPasswordRequest request)
    {
        try
        {
            var user = await _repositoryAdminUser.Get(a => a.Id == request.UserId);
            if (user == null || !(user.Id + "-" + AppSettings.Current.StoreKey).Md5().Equals(request.UToken, StringComparison.OrdinalIgnoreCase))
            {
                return new Result<AdminUser>("Geçersiz link");
            }
            user.Password = request.Password;

            return await SaveAdminUser((AdminUserDto)user);

        }
        catch (Exception ex)
        {
            return new Result<AdminUser>(await _logService.LogException(ex));
        }
    }

    private List<ExcelColumn<AdminUserDto>> GetAdminUserExcelColumns()
    {
        var columns = ExcelColumn<AdminUserDto>.Columns;
        columns.RemoveAll(a => a.Name == Consts.Status);
        columns.RemoveAll(a => a.Name == "AdminRole");
        columns.Add(GetEnumExcelColum<AdminUserDto, Status>(a => a.Status));
        return columns;
    }

    private string RepleceContentString(string? input, AdminUser user)
    {
        input ??= string.Empty;
        input = ReplaceAllFieldValues(input, user);

        var forgotLink = AppSettings.Current.AdminDomain?.TrimEnd('/') + "/home/resetpassword";
        var id = user.Id;
        var token = (user.Id + "-" + AppSettings.Current.StoreKey).Md5();
        if (!forgotLink.Contains('?'))
        {
            forgotLink += "?id=" + id + "&utoken=" + token;
        }
        else
        {
            forgotLink += "&id=" + id + "&utoken=" + token;
        }
        input = input.Replace("[Link]", forgotLink);
        return input;
    }
}
