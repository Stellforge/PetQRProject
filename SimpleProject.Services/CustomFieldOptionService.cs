using SimpleProject.Data;
using SimpleProject.Domain;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleProject.Services;

public interface ICustomFieldOptionService : IServiceBase, IScopedService
{
    Task<Result<CustomFieldOption>> SaveCustomFieldOption(CustomFieldOptionDto data);
    Task<Result> DeleteCustomFieldOption(int id);
    internal Task DeleteCustomFieldOption(CustomFieldOption entity);
    Task<Result<IEnumerable<CustomFieldOptionDto>>> QueryCustomFieldOptionForExport(Query<CustomFieldOption> query, bool? isSample = null);
    Task<Result> SaveCustomFieldOptionExcel(ExcelUploadDto data);
}

public class CustomFieldOptionService : ServiceBase, ICustomFieldOptionService
{
    private ICustomFieldOptionService Self => this;
    private readonly IExcelService _excelService;
    private readonly IRepository<CustomField> _repositoryCustomField;
    private readonly IRepository<CustomFieldOption> _repositoryCustomFieldOption;
    private readonly IRepository<CustomFieldValue> _repositoryCustomFieldValue;

    public CustomFieldOptionService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _excelService = _serviceProvider.GetRequiredService<IExcelService>();
        _repositoryCustomField = _serviceProvider.GetRequiredService<IRepository<CustomField>>();
        _repositoryCustomFieldOption = _serviceProvider.GetRequiredService<IRepository<CustomFieldOption>>();
        _repositoryCustomFieldValue = _serviceProvider.GetRequiredService<IRepository<CustomFieldValue>>();
    }
    
    public async Task<Result<CustomFieldOption>> SaveCustomFieldOption(CustomFieldOptionDto data)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            var validationResult = _validation.Validate(data);
            if (validationResult.HasError)
            {
                return new Result<CustomFieldOption>(validationResult);
            }

            if (!data.CustomFieldId.HasValue && string.IsNullOrEmpty(data.CustomFieldCode) && string.IsNullOrEmpty(data.CustomFieldName))
            {
                return new Result<CustomFieldOption>("Özel alan id, kod ve ad alanlarýndan en az biri olmalýdýr");
            }

            var entity = (CustomFieldOption)data;
            var oldEntity = default(CustomFieldOption);

            if (!data.CustomFieldId.HasValue)
            {
                if (!string.IsNullOrEmpty(data.CustomFieldCode))
                {
                    var customField = await _repositoryCustomField.Get(a => a.Code == data.CustomFieldCode, a => new CustomField() { Id = a.Id }) ?? throw new BusException("Özel alan bulunamaýd");
                    entity.CustomFieldId = customField.Id;
                }
                else if (!string.IsNullOrEmpty(data.CustomFieldName))
                {
                    var customField = await _repositoryCustomField.Get(a => a.Name == data.CustomFieldName, a => new CustomField() { Id = a.Id }) ?? throw new BusException("Özel alan bulunamadý");
                    entity.CustomFieldId = customField.Id;
                }
            }

            if (entity.Id > 0)
            {
                oldEntity = await _repositoryCustomFieldOption.Get(a => a.Id == entity.Id) ?? throw new BusException("Kayýt bulunamadý");

                entity.CreateDate = oldEntity.CreateDate;
                entity.UpdateDate = oldEntity.UpdateDate;
            }

            if (oldEntity != null && entity.CustomFieldId != oldEntity.CustomFieldId)
            {
                throw new BusException("Özel alan id deðiþtirileme");
            }

            if (data.CustomFieldId.HasValue && (oldEntity == null || oldEntity.CustomFieldId != entity.CustomFieldId))
            {
                var exists = await _repositoryCustomField.Any(a => a.Id == entity.CustomFieldId);
                if (!exists)
                {
                    throw new BusException("Özel alan bulunamadý");
                }
            }

            if (oldEntity == null || oldEntity.Name != entity.Name || oldEntity.CustomFieldId != entity.CustomFieldId)
            {
                var exists = await _repositoryCustomFieldOption.Any(a => a.Id != entity.Id && a.Name == entity.Name && a.CustomFieldId == entity.CustomFieldId);
                if (exists)
                {
                    throw new BusException(string.Format("\"{0}\" bu ad ile zaten kayýtlý bir seçenek var", entity.Name));
                }
            }

            if (!string.IsNullOrEmpty(entity.Code) && (oldEntity == null || oldEntity.Code != entity.Code || oldEntity.CustomFieldId != entity.CustomFieldId))
            {
                var exists = await _repositoryCustomFieldOption.Any(a => a.Code == entity.Code && a.CustomFieldId == entity.CustomFieldId && a.Id != entity.Id);
                if (exists)
                {
                    throw new BusException(string.Format("\"{0}\" bu kod ile zaten kayýtlý bir seçenek var", entity.Code));
                }
            }

            if (entity.DisplayOrder == 0)
            {
                entity.DisplayOrder = (await _repositoryCustomFieldOption.Max(a=> a.CustomFieldId == entity.CustomFieldId, a => (int?)a.DisplayOrder)) ?? 0 + 1;
            }

            if (entity.Id > 0)
            {
                await _repositoryCustomFieldOption.Update(entity, oldEntity);
            }
            else
            {
                await _repositoryCustomFieldOption.Add(entity);
            }

            await _logService.LogEntityHistory(entity, oldEntity);

            return new Result<CustomFieldOption>() { Data = entity };
        }
        catch (Exception ex)
        {
            if (!isTransactional)
            {
                return new Result<CustomFieldOption>(await _logService.LogException(ex));
            }
            throw;
        }
    }
    public async Task<Result> DeleteCustomFieldOption(int id)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            if (!isTransactional)
            {
                await _unitOfWork.BeginTransaction();
            }

            var entity = await _repositoryCustomFieldOption.Get(a => a.Id == id, a=> new CustomFieldOption()
            {
                Id = a.Id
            }) ?? throw new BusException("Kayýt bulunamadý");

            await Self.DeleteCustomFieldOption(entity);

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
    async Task ICustomFieldOptionService.DeleteCustomFieldOption(CustomFieldOption entity)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        var customFieldValueService = _serviceProvider.GetRequiredService<ICustomFieldValueService>();

        var customFieldValues = await _repositoryCustomFieldValue.Query(a => a.CustomFieldOptionId == entity.Id);
        foreach (var item in customFieldValues)
        {
            await customFieldValueService.DeleteCustomFieldValue(item);
        }

        await _repositoryCustomFieldOption.Delete(entity);

        await _logService.LogDeleteHistory(entity);
    }
    public async Task<Result<IEnumerable<CustomFieldOptionDto>>> QueryCustomFieldOptionForExport(Query<CustomFieldOption> query, bool? isSample = null)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            var columns = GetCustomFieldOptionExcelColumns();
            query.Select = columns.GetSelect<CustomFieldOptionDto, CustomFieldOption>(a => new CustomFieldOption()
            {
                CustomField = new CustomField()
                {
                    Id = a.CustomField!.Id,
                    Code = a.CustomField!.Code,
                    Name = a.CustomField!.Name
                }
            });
            query.Top = isSample.GetValueOrDefault() ? 5 : 0;

            var data = await _repositoryCustomFieldOption.Query(query);
            return new Result<IEnumerable<CustomFieldOptionDto>>()
            {
                Data = [.. data.Select(a => (CustomFieldOptionDto)a)],
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
                return new Result<IEnumerable<CustomFieldOptionDto>>(await _logService.LogException(ex));
            }
            throw;
        }
    }
    public async Task<Result> SaveCustomFieldOptionExcel(ExcelUploadDto data)
    {
        try
        {
            var columns = GetCustomFieldOptionExcelColumns();
            return await _excelService.SaveExcel(data, columns, SaveCustomFieldOption);
        }
        catch (Exception ex)
        {
            return new Result(await _logService.LogException(ex));
        }
    }

    private List<ExcelColumn<CustomFieldOptionDto>> GetCustomFieldOptionExcelColumns()
    {
        var columns = ExcelColumn<CustomFieldOptionDto>.Columns;

        columns.RemoveAll(a => a.Name == Consts.Status);
        columns.Add(GetEnumExcelColum<CustomFieldOptionDto, Status>(a => a.Status));

        return columns;
    }
}