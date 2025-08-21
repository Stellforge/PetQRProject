using SimpleProject.Data;
using SimpleProject.Domain;
using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleProject.Services;

public interface ICustomFieldValueService : IServiceBase, IScopedService
{
    Task<Result<CustomFieldValue>> SaveCustomFieldValue(CustomFieldValueDto data);
    Task<Result> DeleteCustomFieldValue(int id);
    internal Task DeleteCustomFieldValue(CustomFieldValue entity);
    Task<Result<IEnumerable<CustomFieldValueDto>>> QueryCustomFieldValueForExport(Query<CustomFieldValue> query, bool? isSample = null);
    Task<Result> SaveCustomFieldValueExcel(ExcelUploadDto data);

    internal Result ValidateCustomFieldValue(CustomField customField, CustomFieldValueDto value);
}

public class CustomFieldValueService : ServiceBase, ICustomFieldValueService
{
    private ICustomFieldValueService Self => this;
    private readonly IExcelService _excelService;
    private readonly IRepository<CustomField> _repositoryCustomField;
    private readonly IRepository<CustomFieldOption> _repositoryCustomFieldOption;
    private readonly IRepository<CustomFieldValue> _repositoryCustomFieldValue;

    public CustomFieldValueService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _excelService = _serviceProvider.GetRequiredService<IExcelService>();
        _repositoryCustomField = _serviceProvider.GetRequiredService<IRepository<CustomField>>();
        _repositoryCustomFieldOption = _serviceProvider.GetRequiredService<IRepository<CustomFieldOption>>();
        _repositoryCustomFieldValue = _serviceProvider.GetRequiredService<IRepository<CustomFieldValue>>();
    }
    
    public async Task<Result<CustomFieldValue>> SaveCustomFieldValue(CustomFieldValueDto data)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            var validationResult = _validation.Validate(data);
            if (validationResult.HasError)
            {
                return new Result<CustomFieldValue>(validationResult);
            }

            if (!data.CustomFieldId.HasValue && string.IsNullOrEmpty(data.CustomFieldCode) && string.IsNullOrEmpty(data.CustomFieldName))
            {
                return new Result<CustomFieldValue>("Özel alan id, kod ve ad alanlarýndan en az biri olmalýdýr");
            }

            //if (!data.CustomFieldOptionId.HasValue && string.IsNullOrEmpty(data.CustomFieldOptionCode) && string.IsNullOrEmpty(data.CustomFieldOptionName))
            //{
            //    return new Result<CustomFieldValue>("Özel alan seçenek id, kod ve ad alanlarýndan en az biri olmalýdýr");
            //}

            var entity = (CustomFieldValue)data;
            var oldEntity = default(CustomFieldValue);

            var customField = default(CustomField);
            if (data.CustomFieldId.HasValue)
            {
                customField = await _repositoryCustomField.Get(a => a.Id == data.CustomFieldId, a => new CustomField()
                {
                    Id = a.Id,
                    Name = a.Name,
                    Required = a.Required,
                    FieldType = a.FieldType
                }) ?? throw new BusException("Özel alan bulunamadý");
            }
            else
            {
                if (!string.IsNullOrEmpty(data.CustomFieldCode))
                {
                    customField = await _repositoryCustomField.Get(a => a.Code == data.CustomFieldCode, a => new CustomField()
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Required = a.Required,
                        FieldType = a.FieldType
                    }) ?? throw new BusException("Özel alan bulunamadý");
                    entity.CustomFieldId = customField.Id;
                }
                else if (!string.IsNullOrEmpty(data.CustomFieldName))
                {
                    customField = await _repositoryCustomField.Get(a => a.Name == data.CustomFieldName, a => new CustomField()
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Required = a.Required,
                        FieldType = a.FieldType
                    }) ?? throw new BusException("Özel alan bulunamadý");
                    entity.CustomFieldId = customField.Id;
                }
            }

            if (!data.CustomFieldOptionId.HasValue)
            {
                if (!string.IsNullOrEmpty(data.CustomFieldOptionCode))
                {
                    var customFieldOption = await _repositoryCustomFieldOption.Get(a => a.Code == data.CustomFieldOptionCode, a => new CustomFieldOption() { Id = a.Id }) ?? throw new BusException("Özel alan seçenek bulunamadý");
                    entity.CustomFieldOptionId = customFieldOption.Id;
                }
                else if (!string.IsNullOrEmpty(data.CustomFieldOptionName))
                {
                    var customFieldOption = await _repositoryCustomFieldOption.Get(a => a.Name == data.CustomFieldOptionName, a => new CustomFieldOption() { Id = a.Id }) ?? throw new BusException("Özel alan seçenek bulunamadý");
                    entity.CustomFieldOptionId = customFieldOption.Id;
                }
            }

            if (entity.Id > 0)
            {
                oldEntity = await _repositoryCustomFieldValue.Get(a => a.Id == entity.Id) ?? throw new BusException("Kayýt bulunamaýd");
                
                entity.CreateDate = oldEntity.CreateDate;
                entity.UpdateDate = oldEntity.UpdateDate;
            }

            if (oldEntity != null && entity.CustomFieldId != oldEntity.CustomFieldId)
            {
                throw new BusException("Özel alan deðiþtirilemez");
            }

            if (oldEntity != null && entity.TableId != oldEntity.TableId)
            {
                throw new BusException("Tablo alan deðiþtirilemez");
            }

            if (data.CustomFieldOptionId.HasValue && (oldEntity == null || oldEntity.CustomFieldOptionId != entity.CustomFieldOptionId))
            {
                var exists = await _repositoryCustomFieldOption.Any(a => a.Id == entity.CustomFieldOptionId);
                if (!exists)
                {
                    throw new BusException("Özel alan seçenek bulunamadý");
                }
            }

            var result = Self.ValidateCustomFieldValue(customField!, data);
            if (result.HasError)
            {
                throw new BusException(result);
            }

            if (oldEntity == null || oldEntity.Value != entity.Value || oldEntity.CustomFieldId != entity.CustomFieldId || oldEntity.CustomFieldOptionId != entity.CustomFieldOptionId || oldEntity.TableId != entity.TableId)
            {
                if (entity.CustomFieldOptionId.HasValue)
                {
                    var exists = await _repositoryCustomFieldValue.Any(a => a.CustomFieldId == entity.CustomFieldId && a.CustomFieldOptionId == entity.CustomFieldOptionId && a.TableId == entity.TableId && a.Id != entity.Id);
                    if (exists)
                    {
                        throw new BusException(string.Format("\"{0}\" bu deðer ile zaten bir kayýt var", entity.CustomFieldOptionId));
                    }
                }
                else
                {
                    var exists = await _repositoryCustomFieldValue.Any(a => a.Value == entity.Value && a.CustomFieldId == entity.CustomFieldId && !a.CustomFieldOptionId.HasValue && a.TableId == entity.TableId && a.Id != entity.Id);
                    if (exists)
                    {
                        throw new BusException(string.Format("\"{0}\" bu deðer ile zaten bir kayýt var", entity.Value));
                    }
                }
            }

            if (entity.Id > 0)
            {
                await _repositoryCustomFieldValue.Update(entity, oldEntity);
            }
            else
            {
                await _repositoryCustomFieldValue.Add(entity);
            }

            await _logService.LogEntityHistory(entity, oldEntity);

            return new Result<CustomFieldValue>() { Data = entity };
        }
        catch (Exception ex)
        {
            if (!isTransactional)
            {
                return new Result<CustomFieldValue>(await _logService.LogException(ex));
            }
            throw;
        }
    }
    public async Task<Result> DeleteCustomFieldValue(int id)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            var entity = await _repositoryCustomFieldValue.Get(a => a.Id == id, a=> new CustomFieldValue()
            {
                Id = a.Id
            }) ?? throw new BusException("Kayýt bulunamadý");

            await Self.DeleteCustomFieldValue(entity);

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
    public async Task DeleteCustomFieldValue(CustomFieldValue entity)
    {
        await _repositoryCustomFieldValue.Delete(entity);

        await _logService.LogDeleteHistory(entity);

    }
    public async Task<Result<IEnumerable<CustomFieldValueDto>>> QueryCustomFieldValueForExport(Query<CustomFieldValue> query, bool? isSample = null)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            var columns = GetCustomFieldValueExcelColumns();
            query.Select = columns.GetSelect<CustomFieldValueDto, CustomFieldValue>(a=> new CustomFieldValue()
            {
                CustomField = new CustomField()
                {
                    Id = a.CustomField!.Id,
                    Code = a.CustomField!.Code,
                    Name = a.CustomField!.Name
                },
                CustomFieldOption = a.CustomFieldOptionId.HasValue ? new CustomFieldOption()
                {
                    Id = a.CustomFieldOption!.Id,
                    Code = a.CustomFieldOption!.Code,
                    Name = a.CustomFieldOption!.Name
                } : null
            });
            query.Top = isSample.GetValueOrDefault() ? 5 : 0;

            var data = await _repositoryCustomFieldValue.Query(query);
            return new Result<IEnumerable<CustomFieldValueDto>>()
            {
                Data = [.. data.Select(a => (CustomFieldValueDto)a)],
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
                return new Result<IEnumerable<CustomFieldValueDto>>(await _logService.LogException(ex));
            }
            throw;
        }
    }
    public async Task<Result> SaveCustomFieldValueExcel(ExcelUploadDto data)
    {
        try
        {
            var columns = GetCustomFieldValueExcelColumns();
            return await _excelService.SaveExcel(data, columns, SaveCustomFieldValue);
        }
        catch (Exception ex)
        {
            return new Result(await _logService.LogException(ex));
        }
    }

    Result ICustomFieldValueService.ValidateCustomFieldValue(CustomField customField, CustomFieldValueDto value)
    {
        if (customField.Required)
        {
            if ((customField.FieldType == FieldType.SELECT && !value.CustomFieldOptionId.HasValue && string.IsNullOrEmpty(value.CustomFieldOptionName)) || (customField.FieldType != FieldType.SELECT && string.IsNullOrEmpty(value.Value)))
            {
                return new Result(string.Format("{0} boþ geçilemez", customField.Name));
            }
        }

        if (!string.IsNullOrEmpty(value.Value))
        {
            var isValid = true;
            switch (customField.FieldType)
            {
                case FieldType.DATETIME:
                    isValid = DateTime.TryParse(value.Value, out _);
                    break;
                case FieldType.DATE:
                    isValid = DateTime.TryParse(value.Value, out var _);
                    break;
                case FieldType.INT:
                    isValid = int.TryParse(value.Value, out _);
                    break;
                case FieldType.DECIMAL:
                    isValid = decimal.TryParse(value.Value, out _);
                    break;
                case FieldType.BOOL:
                    isValid = bool.TryParse(value.Value, out _);
                    break;
                case FieldType.STRING:
                    isValid = true;
                    break;
                case FieldType.TEXTAREA:
                    isValid = true;
                    break;
                case FieldType.SELECT:
                    isValid = true;
                    break;
                case FieldType.UNKNOWN:
                    break;
            }
            if (!isValid)
            {
                return new Result(string.Format("{0} için geçersiz deðer", customField.Name));
            }
        }
        return new Result();
    }

    private static List<ExcelColumn<CustomFieldValueDto>> GetCustomFieldValueExcelColumns()
    {
        var columns = ExcelColumn<CustomFieldValueDto>.Columns;
        return columns;
    }
}