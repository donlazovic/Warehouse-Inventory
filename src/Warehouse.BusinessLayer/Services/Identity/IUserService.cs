using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Identity;

namespace Warehouse.BusinessLayer.Services.Identity;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetPagedAsync(UserFilterRequest filter, CancellationToken ct = default);
    Task<UserDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<int> GetPendingCountAsync(CancellationToken ct = default);
    Task RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, int currentUserId, CancellationToken ct = default);
    Task<UserDto> UpdateAsync(int id, UpdateUserRequest request, int currentUserId, CancellationToken ct = default);
    Task<UserDto> ApproveAsync(int id, ApproveUserRequest request, int currentUserId, CancellationToken ct = default);
    Task RejectAsync(int id, CancellationToken ct = default);
    Task ResetPasswordAsync(int id, ResetPasswordRequest request, CancellationToken ct = default);
}
