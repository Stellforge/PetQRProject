using SimpleProject.Domain.Dtos;

namespace SimpleProject.Services;

public interface IUserAccessor
{
    int? AdminUserId { get; }
    AdminUserDto? AdminUser { get; set; }
    string? ClientIP { get; }
    string? RequestLink { get; }
    string WebRootPath { get; }
    IServiceProvider RequestServiceProvider { get; }
    int TimeZoneOffset { get; }

    void Store<T>(string key, T data);
    T? Get<T>(string key);
    void Clear(string? key = null);

    string? GetLocal(string key);
    void StoreLocal(string key, string value, int expires = 30);
}
