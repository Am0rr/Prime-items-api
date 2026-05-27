using PI.BLL.Interfaces;
using PI.DAL.Interfaces;
using PI.BLL.DTOs.Identity;
using PI.DAL.Entities.Identity;

namespace PI.BLL.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtProvider _jwtProvider;

    public AuthService(IUnitOfWork unitOfWork, IJwtProvider jwtProvider)
    {
        _unitOfWork = unitOfWork;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var oldToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (oldToken.IsRevoked || DateTime.UtcNow >= oldToken.ExpiresAt)
            throw new UnauthorizedAccessException("Refresh token has expired or been revoked.");

        var user = await _unitOfWork.Users.GetByIdAsync(oldToken.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with Id {oldToken.UserId} not found");

        oldToken.Revoke();

        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var token = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (!token.IsRevoked)
        {
            token.Revoke();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = _jwtProvider.GenerateAccessToken(user.Id, user.Username, user.Role.ToString());

        var refreshTokenStr = _jwtProvider.GenerateRefreshToken();

        var refreshToken = RefreshToken.Create(
            refreshTokenStr,
            DateTime.UtcNow.AddDays(7),
            user.Id
        );

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenStr,
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role.ToString()
        };
    }
}