using Warehouse.Domain.Common;

namespace Warehouse.Domain.Entities.Identity;

public class PasswordResetToken : BaseEntity
{
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public string? RequestedByIp { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
