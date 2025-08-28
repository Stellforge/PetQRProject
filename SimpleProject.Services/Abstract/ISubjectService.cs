using SimpleProject.Domain.Dtos;

namespace SimpleProject.Services.Abstract;
public interface ISubjectService
{
    Task<SubjectDto?> GetAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<SubjectDto>> SearchAsync(string? type, string? name, CancellationToken ct = default);
    Task<int> CreateAsync(SubjectDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, SubjectDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
