using AutoMapper;
using PI.BLL.Interfaces;
using PI.DAL.Interfaces;
using PI.BLL.DTOs.Identity;
using PI.DAL.Entities.Identity;
using PI.DAL.Enums;

namespace PI.BLL.Services;

public class UserService : BaseService, IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(request);

        if (await _unitOfWork.Users.ExistsByUsernameAsync(request.Username, cancellationToken))
            throw new InvalidOperationException("This username is already taken.");

        if (await _unitOfWork.Users.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new InvalidOperationException("Account with this email address already exists.");

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = User.Create(
            request.Username,
            request.Email,
            passwordHash,
            UserRole.Registered
        );

        _unitOfWork.Users.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserResponse>(user);
    }

    public async Task UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(request);
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"User with Id {id} not found");

        if (request.Username != null && request.Username != user.Username)
        {
            if (await _unitOfWork.Users.ExistsByUsernameAsync(request.Username, cancellationToken))
                throw new InvalidOperationException("This username is already taken.");

            user.ChangeUsername(request.Username);
        }

        if (request.Email != null && request.Email != user.Email)
        {
            if (await _unitOfWork.Users.ExistsByEmailAsync(request.Email, cancellationToken))
                throw new InvalidOperationException("Account with this email address already exists.");

            user.ChangeEmail(request.Email);
        }

        if (request.Role != null)
        {
            var role = Enum.Parse<UserRole>(request.Role, ignoreCase: true);

            if (role != user.Role)
            {
                user.ChangeRole(role);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"User with Id {id} not found");

        _unitOfWork.Users.Delete(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"User with ID {id} not found.");

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<UserResponse> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken)
            ?? throw new KeyNotFoundException($"User with email {email} not found.");

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<IEnumerable<UserResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);

        return _mapper.Map<IEnumerable<UserResponse>>(users);
    }
}