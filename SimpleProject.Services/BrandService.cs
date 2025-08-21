using SimpleProject.Domain.Dtos;
using SimpleProject.Domain.Entities;
using SimpleProject.Domain;
using SimpleProject.Data;
using Microsoft.Extensions.DependencyInjection;
using SimpleProject.Domain.Enums;

namespace SimpleProject.Services;

public interface IBrandService : IServiceBase, IScopedService
{
    Task<Result<Brand>> SaveBrand(BrandDto data);
    Task<Result> DeleteBrand(int id);
    internal Task DeleteBrand(Brand entity);
    Task<Result<IEnumerable<BrandDto>>> QueryBrandForExport(Query<Brand> query, bool? isSample = null);
    Task<Result> SaveBrandExcel(ExcelUploadDto data);
}
public class BrandService : ServiceBase, IBrandService
{
    private IBrandService Self => this;
    private readonly IExcelService _excelService;
    private readonly IFileService _fileService;
    private readonly IRepository<Brand> _repositoryBrand;

    public BrandService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _excelService = _serviceProvider.GetRequiredService<IExcelService>();
        _fileService = _serviceProvider.GetRequiredService<IFileService>();
        _repositoryBrand = _serviceProvider.GetRequiredService<IRepository<Brand>>();
    }

    public async Task<Result<Brand>> SaveBrand(BrandDto data)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            var validationResult = _validation.Validate(data);
            if (validationResult.HasError)
            {
                return new Result<Brand>(validationResult);
            }

            var entity = (Brand)data;
            var oldEntity = default(Brand);

            if (entity.Id > 0)
            {
                oldEntity = await _repositoryBrand.Get(a => a.Id == entity.Id) ?? throw new BusException("Kayıt bulunamaıd");

                entity.CreateDate = oldEntity.CreateDate;
                entity.UpdateDate = oldEntity.UpdateDate;
            }

            if (oldEntity == null || oldEntity.Name != entity.Name)
            {
                var exists = await _repositoryBrand.Any(a => a.Name == entity.Name && a.Id != entity.Id);
                if (exists)
                {
                    throw new BusException(string.Format("\"{0}\" bu ad ile zaten kayıtlı bir marka var", entity.Name));
                }
            }

            if (!string.IsNullOrEmpty(entity.Code) && (oldEntity == null || oldEntity.Code != entity.Code))
            {
                var exists = await _repositoryBrand.Any(a => a.Code == entity.Code && a.Id != entity.Id);
                if (exists)
                {
                    throw new BusException(string.Format("\"{0}\" bu kod ile zaten kayıtlı bir marka var", entity.Code));
                }
            }

            if (entity.DisplayOrder == 0)
            {
                entity.DisplayOrder = (await _repositoryBrand.Max(filter: null, a => (int?)a.DisplayOrder)) ?? 0 + 1;
            }

            if (entity.Id > 0)
            {
                await _repositoryBrand.Update(entity, oldEntity);
            }
            else
            {
                await _repositoryBrand.Add(entity);
            }

            await _logService.LogEntityHistory(entity, oldEntity);

            if (oldEntity != null && !string.IsNullOrEmpty(oldEntity.Image) && oldEntity.Image != entity.Image)
            {
                await _fileService.DeleteFile(oldEntity.Image, true);
            }

            if (oldEntity != null && !string.IsNullOrEmpty(oldEntity.Thumbnail) && oldEntity.Thumbnail != entity.Thumbnail)
            {
                await _fileService.DeleteFile(oldEntity.Thumbnail, true);
            }

            return new Result<Brand>() { Data = entity };
        }
        catch (Exception ex)
        {
            if (!isTransactional)
            {
                return new Result<Brand>(await _logService.LogException(ex));
            }
            throw;
        }
    }
    public async Task<Result> DeleteBrand(int id)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
           var entity = await _repositoryBrand.Get(a => a.Id == id, a=> new Brand()
            {
                Id = a.Id,
                Image = a.Image,
                Thumbnail = a.Thumbnail
            }) ?? throw new BusException("Kayıt bulunamaıd");

            await Self.DeleteBrand(entity);

            if (!string.IsNullOrEmpty(entity.Image))
            {
                await _fileService.DeleteFile(entity.Image, true);
            }

            if (!string.IsNullOrEmpty(entity.Thumbnail))
            {
                await _fileService.DeleteFile(entity.Thumbnail, true);
            }

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
    async Task IBrandService.DeleteBrand(Brand entity)
    {
        await _repositoryBrand.Delete(entity);

        await _logService.LogDeleteHistory(entity);
    }
    public async Task<Result<IEnumerable<BrandDto>>> QueryBrandForExport(Query<Brand> query, bool? isSample = null)
    {
        var isTransactional = _unitOfWork.IsTransactional();
        try
        {
            var columns = GetBrandExcelColumns();
            query.Select = columns.GetSelect<BrandDto, Brand>(a => new Brand());
            query.Top = isSample.GetValueOrDefault() ? 5 : 0;

            var data = await _repositoryBrand.Query(query);
            return new Result<IEnumerable<BrandDto>>()
            {
                Data = [.. data.Select(a => (BrandDto)a)],
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
                return new Result<IEnumerable<BrandDto>>(await _logService.LogException(ex));
            }
            throw;
        }
    }
    public async Task<Result> SaveBrandExcel(ExcelUploadDto data)
    {
        try
        {
            var columns = GetBrandExcelColumns();
            return await _excelService.SaveExcel(data, columns, SaveBrand);
        }
        catch (Exception ex)
        {
            return new Result(await _logService.LogException(ex));
        }
    }

    private List<ExcelColumn<BrandDto>> GetBrandExcelColumns()
    {
        var columns = ExcelColumn<BrandDto>.Columns;
        columns.RemoveAll(a => a.Name == Consts.Status);
        columns.Add(GetEnumExcelColum<BrandDto, Status>(a => a.Status));
        return columns;
    }
}
