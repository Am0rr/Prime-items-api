using PI.BLL.DTOs.Identity;

namespace PI.BLL.Interfaces;

public interface IUserService
{
    Task<Guid> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserResponse?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<List<UserResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}