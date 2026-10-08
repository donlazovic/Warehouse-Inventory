using Microsoft.EntityFrameworkCore;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Identity;
using Warehouse.BusinessLayer.Realtime;
using Warehouse.DataLayer.Repositories;
using Warehouse.Domain.Entities.Identity;

namespace Warehouse.BusinessLayer.Services.Identity;

public class UserService : IUserService
{
    private const string AdminRoleName = "Admin";
    private const string DefaultPendingRoleName = "Viewer";
    private const int MinPasswordLength = 8;

    private readonly IUnitOfWork _uow;
    private readonly IEventCollector _events;

    public UserService(IUnitOfWork uow, IEventCollector events)
    {
        _uow = uow;
        _events = events;
    }

    public async Task<PagedResult<UserDto>> GetPagedAsync(UserFilterRequest filter, CancellationToken ct = default)
    {
        var query = _uow.Repository<User>().Query();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(x => x.FirstName.Contains(term)
                                  || x.LastName.Contains(term)
                                  || x.Email.Contains(term));
        }

        if (filter.RoleId.HasValue)
            query = query.Where(x => x.RoleId == filter.RoleId);

        query = filter.Status switch
        {
            UserStatusFilter.Pending => query.Where(x => x.ApprovedAt == null),
            UserStatusFilter.Active => query.Where(x => x.ApprovedAt != null && x.IsActive),
            UserStatusFilter.Inactive => query.Where(x => x.ApprovedAt != null && !x.IsActive),
            _ => query
        };

        query = filter.SortBy?.ToLowerInvariant() switch
        {
            "email" => query.ApplySort(x => x.Email, filter.SortDesc),
            "role" => query.ApplySort(x => x.Role.Name, filter.SortDesc),
            "createdat" => query.ApplySort(x => x.CreatedAt, filter.SortDesc),
            _ => query.ApplySort(x => x.LastName, filter.SortDesc)
        };

        return await Project(query).ToPagedResultAsync(filter, ct);
    }

    public async Task<UserDto> GetByIdAsync(int id, CancellationToken ct = default)
        => await Project(_uow.Repository<User>().Query().Where(x => x.Id == id)).FirstOrDefaultAsync(ct)
           ?? throw new AppException("Korisnik nije pronadjen.", 404);

    public Task<int> GetPendingCountAsync(CancellationToken ct = default)
        => _uow.Repository<User>().Query().CountAsync(x => x.ApprovedAt == null, ct);

    public async Task RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = NormalizeEmail(request.Email);
        var firstName = Guard.Required(request.FirstName, "Ime", 100);
        var lastName = Guard.Required(request.LastName, "Prezime", 100);
        ValidatePassword(request.Password);

        if (await _uow.Repository<User>().ExistsAsync(x => x.Email == email, ct))
            return;

        var pendingRole = await _uow.Repository<Role>()
            .Query()
            .FirstOrDefaultAsync(x => x.Name == DefaultPendingRoleName, ct)
            ?? throw new AppException("Sistemska uloga za nove naloge ne postoji.", 500);

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = pendingRole.Id,
            IsActive = true,
            ApprovedAt = null
        };

        await _uow.Repository<User>().AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        _events.Add(new UserRegisteredEvent(user.Id));
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, int currentUserId, CancellationToken ct = default)
    {
        var email = NormalizeEmail(request.Email);
        var firstName = Guard.Required(request.FirstName, "Ime", 100);
        var lastName = Guard.Required(request.LastName, "Prezime", 100);
        ValidatePassword(request.Password);

        if (await _uow.Repository<User>().ExistsAsync(x => x.Email == email, ct))
            throw new AppException("Korisnik sa tom email adresom vec postoji.");

        if (!await _uow.Repository<Role>().ExistsAsync(x => x.Id == request.RoleId, ct))
            throw new AppException("Izabrana uloga ne postoji.");

        await EnsureCanGrantRoleAsync(request.RoleId, currentUserId, ct);

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = request.RoleId,
            IsActive = true,
            ApprovedAt = DateTime.UtcNow,
            ApprovedByUserId = currentUserId
        };

        await _uow.Repository<User>().AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(user.Id, ct);
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserRequest request, int currentUserId, CancellationToken ct = default)
    {
        var repo = _uow.Repository<User>();

        var user = await repo.Query(asNoTracking: false)
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Korisnik nije pronadjen.", 404);

        await EnsureCanManageAsync(user, currentUserId, ct);

        if (!await _uow.Repository<Role>().ExistsAsync(x => x.Id == request.RoleId, ct))
            throw new AppException("Izabrana uloga ne postoji.");

        if (id == currentUserId && !request.IsActive)
            throw new AppException("Ne mozete deaktivirati sopstveni nalog.");

        if (id == currentUserId && request.RoleId != user.RoleId)
            throw new AppException("Ne mozete promeniti sopstvenu ulogu.");

        if (request.RoleId != user.RoleId)
            await EnsureCanGrantRoleAsync(request.RoleId, currentUserId, ct);

        if (user.Role.Name == AdminRoleName && (request.RoleId != user.RoleId || !request.IsActive))
            await EnsureNotLastAdminAsync(id, ct);

        user.FirstName = Guard.Required(request.FirstName, "Ime", 100);
        user.LastName = Guard.Required(request.LastName, "Prezime", 100);
        user.RoleId = request.RoleId;

        if (user.IsActive && !request.IsActive)
            await RevokeTokensAsync(id, ct);

        user.IsActive = request.IsActive;

        repo.Update(user);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task<UserDto> ApproveAsync(int id, ApproveUserRequest request, int currentUserId, CancellationToken ct = default)
    {
        var repo = _uow.Repository<User>();

        var user = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Korisnik nije pronadjen.", 404);

        if (user.ApprovedAt is not null)
            throw new AppException("Korisnik je vec odobren.");

        if (!await _uow.Repository<Role>().ExistsAsync(x => x.Id == request.RoleId, ct))
            throw new AppException("Izabrana uloga ne postoji.");

        await EnsureCanGrantRoleAsync(request.RoleId, currentUserId, ct);

        user.RoleId = request.RoleId;
        user.IsActive = true;
        user.ApprovedAt = DateTime.UtcNow;
        user.ApprovedByUserId = currentUserId;

        repo.Update(user);
        await _uow.SaveChangesAsync(ct);

        _events.Add(new PendingUsersChangedEvent());

        return await GetByIdAsync(id, ct);
    }

    public async Task RejectAsync(int id, CancellationToken ct = default)
    {
        var repo = _uow.Repository<User>();

        var user = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Korisnik nije pronadjen.", 404);

        if (user.ApprovedAt is not null)
            throw new AppException("Odobren nalog se ne moze odbiti. Deaktivirajte ga umesto toga.");

        repo.Remove(user);
        await _uow.SaveChangesAsync(ct);

        _events.Add(new PendingUsersChangedEvent());
    }

    public async Task ResetPasswordAsync(int id, ResetPasswordRequest request, int currentUserId, CancellationToken ct = default)
    {
        if (id == currentUserId)
            throw new AppException("Sopstvenu lozinku ne mozete resetovati ovde.");

        ValidatePassword(request.NewPassword);

        var repo = _uow.Repository<User>();

        var user = await repo.Query(asNoTracking: false)
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Korisnik nije pronadjen.", 404);

        await EnsureCanManageAsync(user, currentUserId, ct);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        repo.Update(user);

        await RevokeTokensAsync(id, ct);
        await _uow.SaveChangesAsync(ct);
    }

    private async Task EnsureCanManageAsync(User target, int currentUserId, CancellationToken ct)
    {
        if (target.Id == currentUserId)
            return;

        if (target.IsOwner)
            throw new AppException("Vlasnika sistema niko ne moze menjati.", 403);

        if (target.Role.Name == AdminRoleName && !await IsOwnerAsync(currentUserId, ct))
            throw new AppException("Samo vlasnik sistema moze menjati druge administratore.", 403);
    }

    private async Task EnsureCanGrantRoleAsync(int roleId, int currentUserId, CancellationToken ct)
    {
        var isAdminRole = await _uow.Repository<Role>()
            .ExistsAsync(x => x.Id == roleId && x.Name == AdminRoleName, ct);

        if (isAdminRole && !await IsOwnerAsync(currentUserId, ct))
            throw new AppException("Samo vlasnik sistema moze dodeliti ulogu administratora.", 403);
    }

    private Task<bool> IsOwnerAsync(int userId, CancellationToken ct)
        => _uow.Repository<User>().ExistsAsync(x => x.Id == userId && x.IsOwner, ct);

    private async Task RevokeTokensAsync(int userId, CancellationToken ct)
    {
        var tokens = await _uow.Repository<RefreshToken>()
            .Query(asNoTracking: false)
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            _uow.Repository<RefreshToken>().Update(token);
        }
    }

    private async Task EnsureNotLastAdminAsync(int excludedUserId, CancellationToken ct)
    {
        var remainingAdmins = await _uow.Repository<User>()
            .Query()
            .CountAsync(x => x.Role.Name == AdminRoleName && x.IsActive && x.Id != excludedUserId, ct);

        if (remainingAdmins == 0)
            throw new AppException("Sistem mora imati bar jednog aktivnog administratora.");
    }

    private static string NormalizeEmail(string? email) => Guard.Email(email, required: true)!.ToLowerInvariant();

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
            throw new AppException($"Lozinka mora imati najmanje {MinPasswordLength} karaktera.");
    }

    private static IQueryable<UserDto> Project(IQueryable<User> query)
        => query.Select(x => new UserDto(
            x.Id,
            x.FirstName,
            x.LastName,
            x.Email,
            x.RoleId,
            x.Role.Name,
            x.IsActive,
            x.ApprovedAt == null,
            x.ApprovedAt,
            x.ApprovedByUser != null ? x.ApprovedByUser.FirstName + " " + x.ApprovedByUser.LastName : null,
            x.LastLoginAt,
            x.CreatedAt,
            x.IsOwner));
}
