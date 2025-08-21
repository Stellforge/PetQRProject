using SimpleProject.Data;
using SimpleProject.Domain;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleProject.Services;

public interface ICustomFieldService : IServiceBase, IScopedService
{
    Task<Result<CustomField>> SaveCustomField(CustomFieldDto data);
    Task<Result> DeleteCustomField(int id);
    internal Task DeleteCustomField(CustomField entity);
    Task<Result<IEnumerable<CustomFieldDto>>> QueryCustomFieldForExport(Query<CustomField> query, bool? isSample = null);
    Task<Result> SaveCustomFieldExcel(ExcelUploadDto data);
}

public class CustomFieldService : ServiceBase, ICustomFieldService
{
    private ICustomFieldService Self => this;
    private readonly IExcelService _excelService;
    private readonly IRepository<CustomField> _repositoryCustomField;
    private readonly IRepository<CustomFieldOption> _repositoryCustomFieldOption;
    private readonly IRepository<CustomFieldValue> _repositoryCustomFieldValue;

    public CustomFieldService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _excelService = _serviceProvider.GetRequiredService<IExcelService>();
        _repositoryCustomField = _serviceProvider.GetRequiredService<IRepository<CustomField>>();
        _repositoryCustomFieldOption = _serviceProvider.GetRequiredService<IRepository<CustomFieldOption>>();
        _repositoryCustomFieldValue = _serviceProvider.GetRequiredService<IRepository<CustomFieldValue>>();
    }

    public async Task<Result<CustomField>> SaveCustomField(CustomFieldDto data)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            var validationResult = _validation.Validate(data);
            if (validationResult.HasError)
            {
                return new Result<CustomField>(validationResult);
            }

            if (!isTransactional)
            {
                await _unitOfWork.BeginTransaction();
            }

            var entity = (CustomField)data;
            var oldEntity = default(CustomField);

            if (entity.Id > 0)
            {
                oldEntity = await _repositoryCustomField.Get(a => a.Id == entity.Id) ?? throw new BusException("Kayýt bulunamadý");

                entity.CreateDate = oldEntity.CreateDate;
                entity.UpdateDate = oldEntity.UpdateDate;
            }

            if (oldEntity != null && entity.TableName != oldEntity.TableName)
            {
                throw new BusException("Tablo deðiþtirilemez");
            }

            if (oldEntity == null || oldEntity.Name != entity.Name || oldEntity.TableName != entity.TableName)
            {
                var exists = await _repositoryCustomField.Any(a => a.Id != entity.Id && a.Name == entity.Name && a.TableName == entity.TableName);
                if (exists)
                {
                    throw new BusException(string.Format("\"{0}\" bu ad ile zaten kayýtlý bir özel alan var", entity.Name));
                }
            }

            if (!string.IsNullOrEmpty(entity.Code) && (oldEntity == null || oldEntity.Code != entity.Code || oldEntity.TableName != entity.TableName))
            {
                var exists = await _repositoryCustomField.Any(a => a.Code == entity.Code && a.TableName == entity.TableName && a.Id != entity.Id);
                if (exists)
                {
                    throw new BusException(string.Format("\"{0}\" bu kod ile zaten kayýtlý bir özel alan var", entity.Code));
                }
            }

            if (entity.DisplayOrder == 0)
            {
                entity.DisplayOrder = (await _repositoryCustomField.Max(a=> a.TableName == entity.TableName, a => (int?)a.DisplayOrder)) ?? 0 + 1;
            }

            if (entity.Id > 0)
            {
                await _repositoryCustomField.Update(entity, oldEntity);
            }
            else
            {
                await _repositoryCustomField.Add(entity);
            }

            await _logService.LogEntityHistory(entity, oldEntity);

            var customFieldOptionService = _serviceProvider.GetRequiredService<ICustomFieldOptionService>();
            if (data.CustomFieldOptions != null)
            {
                if (oldEntity != null)
                {
                    var oldOptions = await _repositoryCustomFieldOption.Query(a => a.CustomFieldId == entity.Id, a=> new CustomFieldOption()
                    {
                        Id = a.Id,
                        Name = a.Name
                    });
                    foreach (var item in data.CustomFieldOptions)
                    {
                        if (item.Id == 0)
                        {
                            var oldOption = oldOptions.FirstOrDefault(a => string.Equals(a.Name, item.Name, StringComparison.InvariantCultureIgnoreCase));
                            if (oldOption != null)
                            {
                                item.Id = oldOption.Id;
                            }
                        }
                        else if (!oldOptions.Any(a => a.Id == item.Id))
                        {
                            throw new BusException(string.Format("\"{0}\" geçersiz deðer", item.Name));
                        }
                        item.CustomFieldId = entity.Id;
                    }

                    foreach (var item in oldOptions.Where(a => !data.CustomFieldOptions.Any(b => a.Id == b.Id)))
                    {
                        await customFieldOptionService.DeleteCustomFieldOption(item);
                    }

                    foreach (var item in data.CustomFieldOptions)
                    {
                        var saveResult = await customFieldOptionService.SaveCustomFieldOption(item);
                        if (saveResult.HasError)
                        {
                            throw new BusException(saveResult);
                        }
                    }
                }
                else
                {
                    foreach (var item in data.CustomFieldOptions)
                    {
                        item.CustomFieldId = entity.Id;
                        var saveResult = await customFieldOptionService.SaveCustomFieldOption(item);
                        if (saveResult.HasError)
                        {
                            throw new BusException(saveResult);
                        }
                    }
                }
            }

            if (!isTransactional)
            {
                await _unitOfWork.CommitTransaction();
                await _logService.WriteEntityHistories();
            }

            return new Result<CustomField>() { Data = entity };
        }
        catch (Exception ex)
        {
            if (!isTransactional)
            {
                await _unitOfWork.RollbackTransaction();
                return new Result<CustomField>(await _logService.LogException(ex));
            }
            throw;
        }
    }
    public async Task<Result> DeleteCustomField(int id)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            if (!isTransactional)
            {
                await _unitOfWork.BeginTransaction();
            }

            var entity = await _repositoryCustomField.Get(a => a.Id == id, a=> new CustomField()
            {
                Id = a.Id
            }) ?? throw new BusException("Kayýt bulunamadý");

            await Self.DeleteCustomField(entity);

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
    async Task ICustomFieldService.DeleteCustomField(CustomField entity)
    {
        var customFielOptionService = _serviceProvider.GetRequiredService<ICustomFieldOptionService>();
        var customFielValueService = _serviceProvider.GetRequiredService<ICustomFieldValueService>();

        var customFieldOptions = await _repositoryCustomFieldOption.Query(a => a.CustomFieldId == entity.Id);
        foreach (var item in customFieldOptions)
        {
            await customFielOptionService.DeleteCustomFieldOption(item);
        }

        var customFieldValues = await _repositoryCustomFieldValue.Query(a => a.CustomFieldId == entity.Id);
        foreach (var item in customFieldValues)
        {
            await customFielValueService.DeleteCustomFieldValue(item);
        }

        await _repositoryCustomField.Delete(entity);

        await _logService.LogDeleteHistory(entity);
    }
    public async Task<Result<IEnumerable<CustomFieldDto>>> QueryCustomFieldForExport(Query<CustomField> query, bool? isSample = null)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            var columns = GetCustomFieldExcelColumns();
            query.Select = columns.GetSelect<CustomFieldDto, CustomField>();
            query.Top = isSample.GetValueOrDefault() ? 5 : 0;

            var data = await _repositoryCustomField.Query(query);
            return new Result<IEnumerable<CustomFieldDto>>()
            {
                Data = [.. data.Select(a => (CustomFieldDto)a)],
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
                return new Result<IEnumerable<CustomFieldDto>>(await _logService.LogException(ex));
            }
            throw;
        }
    }
    public async Task<Result> SaveCustomFieldExcel(ExcelUploadDto data)
    {
        try
        {
            var columns = GetCustomFieldExcelColumns();
            return await _excelService.SaveExcel(data, columns, SaveCustomField);
        }
        catch (Exception ex)
        {
            return new Result(await _logService.LogException(ex));
        }
    }
    
    private List<ExcelColumn<CustomFieldDto>> GetCustomFieldExcelColumns()
    {
        var columns = ExcelColumn<CustomFieldDto>.Columns;

        columns.RemoveAll(a => a.Name == Consts.Status);
        columns.Add(GetEnumExcelColum<CustomFieldDto, Status>(a => a.Status));

        columns.RemoveAll(a => a.Name == "FieldType");
        columns.Add(GetEnumExcelColum<CustomFieldDto, FieldType>(a => a.FieldType));

        return columns;
    }
}