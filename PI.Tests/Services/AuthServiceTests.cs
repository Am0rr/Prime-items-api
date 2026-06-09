using Moq;
using PI.BLL.DTOs.Identity;
using PI.BLL.Interfaces;
using PI.BLL.Services;
using PI.DAL.Entities.Identity;
using PI.DAL.Interfaces;
using PI.DAL.Enums;

namespace PI.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.RefreshTokens).Returns(_refreshTokenRepositoryMock.Object);

        _serviceProviderMock
            .Setup(x => x.GetService(It.IsAny<Type>()))
            .Returns((object?)null);

        _authService = new AuthService(
            _unitOfWorkMock.Object,
            _jwtProviderMock.Object,
            _serviceProviderMock.Object);
    }

    [Fact]
    public async Task LoginAsync_SuccessPath_ReturnsAuthResponse()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = User.Create("testuser", "test@test.com", passwordHash, UserRole.Registered);
        var request = new LoginRequest("test@test.com", "password123");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtProviderMock
            .Setup(j => j.GenerateAccessToken(user.Id, user.Username, user.Role.ToString()))
            .Returns("access-token");

        _jwtProviderMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns(new RefreshTokenResult("refresh-token", DateTime.UtcNow.AddDays(7)));

        var result = await _authService.LoginAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(user.Username, result.Username);

        _refreshTokenRepositoryMock.Verify(r => r.Add(It.IsAny<RefreshToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        var request = new LoginRequest("notfound@test.com", "password123");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(request, CancellationToken.None));

        _refreshTokenRepositoryMock.Verify(r => r.Add(It.IsAny<RefreshToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        var user = User.Create("testuser", "test@test.com", passwordHash, UserRole.Registered);
        var request = new LoginRequest("test@test.com", "wrongpassword");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(request, CancellationToken.None));

        _refreshTokenRepositoryMock.Verify(r => r.Add(It.IsAny<RefreshToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_SuccessPath_RevokesOldAndReturnsNewTokens()
    {
        var user = User.Create("testuser", "test@test.com", "hash", UserRole.Registered);
        var oldToken = RefreshToken.Create("old-token", DateTime.UtcNow.AddDays(7), user.Id);

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync("old-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldToken);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtProviderMock
            .Setup(j => j.GenerateAccessToken(user.Id, user.Username, user.Role.ToString()))
            .Returns("new-access-token");

        _jwtProviderMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns(new RefreshTokenResult("new-refresh-token", DateTime.UtcNow.AddDays(7)));

        var result = await _authService.RefreshAsync("old-token", CancellationToken.None);

        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal("new-refresh-token", result.RefreshToken);
        Assert.True(oldToken.IsRevoked);

        _refreshTokenRepositoryMock.Verify(r => r.Add(It.IsAny<RefreshToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_TokenNotFound_ThrowsUnauthorizedAccessException()
    {
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync("invalid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken)null!);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.RefreshAsync("invalid-token", CancellationToken.None));
    }

    [Fact]
    public async Task RefreshAsync_TokenRevoked_ThrowsUnauthorizedAccessException()
    {
        var token = RefreshToken.Create("revoked-token", DateTime.UtcNow.AddDays(7), Guid.NewGuid());
        token.Revoke();

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync("revoked-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.RefreshAsync("revoked-token", CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_TokenExpired_ThrowsUnauthorizedAccessException()
    {
        var token = RefreshToken.Create("expired-token", DateTime.UtcNow.AddDays(-1), Guid.NewGuid());

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync("expired-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.RefreshAsync("expired-token", CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeAsync_SuccessPath_RevokesTokenAndSaves()
    {
        var token = RefreshToken.Create("active-token", DateTime.UtcNow.AddDays(7), Guid.NewGuid());

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync("active-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        await _authService.RevokeAsync("active-token", CancellationToken.None);

        Assert.True(token.IsRevoked);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeAsync_TokenNotFound_ThrowsUnauthorizedAccessException()
    {
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync("invalid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken)null!);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.RevokeAsync("invalid-token", CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeAsync_TokenAlreadyRevoked_DoesNotSave()
    {
        var token = RefreshToken.Create("revoked-token", DateTime.UtcNow.AddDays(7), Guid.NewGuid());
        token.Revoke();

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync("revoked-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        await _authService.RevokeAsync("revoked-token", CancellationToken.None);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
