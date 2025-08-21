using SimpleProject.Data;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Domain;
using Microsoft.Extensions.DependencyInjection;
using SimpleProject.Domain.Enums;

namespace SimpleProject.Services;

public interface IAdminRoleService : IServiceBase, IScopedService
{
    Task<Result<AdminRole>> SaveAdminRole(AdminRoleDto data);
    Task<Result> DeleteAdminRole(int id);
    internal Task DeleteAdminRole(AdminRole entity);
}

public class AdminRoleService : ServiceBase, IAdminRoleService
{
    private IAdminRoleService Self => this;
    private readonly IRepository<AdminRole> _repositoryAdminRole;
    private readonly IRepository<AdminUser> _repositoryAdminUser;

    public AdminRoleService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _repositoryAdminRole = _serviceProvider.GetRequiredService<IRepository<AdminRole>>();
        _repositoryAdminUser = _serviceProvider.GetRequiredService<IRepository<AdminUser>>();
    }

    public async Task<Result<AdminRole>> SaveAdminRole(AdminRoleDto data)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            var validationResult = _validation.Validate(data);
            if (validationResult.HasError)
            {
                return new Result<AdminRole>(validationResult);
            }

            var entity = (AdminRole)data;
            var oldEntity = default(AdminRole);

            if (entity.Id > 0)
            {
                oldEntity = await _repositoryAdminRole.Get(a => a.Id == entity.Id) ?? throw new BusException("Kayıt bulunamadı");

                entity.CreateDate = oldEntity.CreateDate;
                entity.UpdateDate = oldEntity.UpdateDate;
            }

            if (oldEntity == null || oldEntity.Name != entity.Name)
            {
                var exists = await _repositoryAdminRole.Any(a => a.Name == entity.Name && a.Id != entity.Id);
                if (exists)
                {
                    throw new BusException(string.Format("{0} bu ad ile zaten kayıtlı bir rol var", entity.Name));
                }
            }

            if(!string.IsNullOrEmpty(entity.Code) && (oldEntity == null || oldEntity.Code != entity.Code))
            {
                var exists = await _repositoryAdminRole.Any(a => a.Code == entity.Code && a.Id != entity.Id);
                if (exists)
                {
                    throw new BusException(string.Format("{0} bu kod ile zaten kayıtlı bir rol var", entity.Code));
                }
            }

            if (entity.Id > 0)
            {
                await _repositoryAdminRole.Update(entity, oldEntity);
            }
            else
            {
                await _repositoryAdminRole.Add(entity);
            }

            await _logService.LogEntityHistory(entity, oldEntity);

            return new Result<AdminRole>() { Data = entity };
        }
        catch (Exception ex)
        {
            if (!isTransactional)
            {
                return new Result<AdminRole>(await _logService.LogException(ex));
            }
            throw;
        }
    }
    public async Task<Result> DeleteAdminRole(int id)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            var entity = await _repositoryAdminRole.Get(a => a.Id == id, a=> new AdminRole()
            {
                Id = a.Id
            }) ?? throw new BusException("Kayıt bulunamadı");

            await Self.DeleteAdminRole(entity);

            return new Result();
        }
        catch (Exception ex)
        {
            if (!isTransactional)
            {
                return new Result(await _logService.LogException(ex));
            }
            throw;
        }
    }
    async Task IAdminRoleService.DeleteAdminRole(AdminRole entity)
    {
        var exists = await _repositoryAdminUser.Any(a => a.AdminRoleId == entity.Id);
        if (exists)
        {
            throw new BusException("Kayıt silinemedi. Bu kayıda ait kullancılar bulunmaktadır.");
        }

        await _repositoryAdminRole.Delete(entity);

        await _logService.LogDeleteHistory(entity);
    }
}
