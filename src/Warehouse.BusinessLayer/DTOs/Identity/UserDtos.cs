using Warehouse.BusinessLayer.Common;

namespace Warehouse.BusinessLayer.DTOs.Identity;

public enum UserStatusFilter
{
    All = 0,
    Pending = 1,
    Active = 2,
    Inactive = 3
}

public class UserFilterRequest : PagedRequest
{
    public int? RoleId { get; set; }
    public UserStatusFilter? Status { get; set; }
}

public record UserDto(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    int RoleId,
    string RoleName,
    bool IsActive,
    bool IsPendingApproval,
    DateTime? ApprovedAt,
    string? ApprovedByName,
    DateTime? LastLoginAt,
    DateTime CreatedAt);

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password);

public record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    int RoleId);

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    int RoleId,
    bool IsActive);

public record ApproveUserRequest(int RoleId);

public record ResetPasswordRequest(string NewPassword);
