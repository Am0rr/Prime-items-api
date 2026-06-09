using AutoMapper;
using Moq;
using PI.BLL.DTOs.Identity;
using PI.BLL.Services;
using PI.DAL.Entities.Identity;
using PI.DAL.Enums;
using PI.DAL.Interfaces;

namespace PI.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        _serviceProviderMock
            .Setup(x => x.GetService(It.IsAny<Type>()))
            .Returns((object?)null);

        _userService = new UserService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _serviceProviderMock.Object);
    }

    [Fact]
    public async Task CreateAsync_SuccessPath_AddsUserAndSaves()
    {
        var request = new CreateUserRequest("username", "email@test.com", "Password123!");
        var expectedResponse = new UserResponse { Id = Guid.NewGuid(), Username = request.Username, Email = request.Email };

        _userRepositoryMock.Setup(r => r.ExistsByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mapperMock.Setup(m => m.Map<UserResponse>(It.IsAny<User>()))
            .Returns(expectedResponse);

        var result = await _userService.CreateAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Username, result.Username);

        _userRepositoryMock.Verify(r => r.ExistsByUsernameAsync(request.Username, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(r => r.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(r => r.Add(It.Is<User>(u => u.Username == request.Username && u.Email == request.Email)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateUsername_ThrowsInvalidOperationException()
    {
        var request = new CreateUserRequest("existing", "email@test.com", "Password123!");

        _userRepositoryMock.Setup(r => r.ExistsByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateAsync(request, CancellationToken.None));

        _userRepositoryMock.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        var request = new CreateUserRequest("username", "existing@test.com", "Password123!");

        _userRepositoryMock.Setup(r => r.ExistsByUsernameAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateAsync(request, CancellationToken.None));

        _userRepositoryMock.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_SuccessPath_UpdatesUsernameAndEmail_SavesChanges()
    {
        var id = Guid.NewGuid();
        var user = User.Create("oldname", "old@test.com", "hash", UserRole.Registered);
        var request = new UpdateUserRequest("newname", "new@test.com", null);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.ExistsByUsernameAsync(request.Username!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync(request.Email!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _userService.UpdateAsync(id, request, CancellationToken.None);

        Assert.Equal(request.Username, user.Username);
        Assert.Equal(request.Email, user.Email);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_FailNotFound_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        var request = new UpdateUserRequest("newname", null, null);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.UpdateAsync(id, request, CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateUsername_ThrowsInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var user = User.Create("oldname", "old@test.com", "hash", UserRole.Registered);
        var request = new UpdateUserRequest("takenname", null, null);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.ExistsByUsernameAsync(request.Username!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.UpdateAsync(id, request, CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_SuccessPath_DeletesUserAndSaves()
    {
        var id = Guid.NewGuid();
        var user = User.Create("username", "email@test.com", "hash", UserRole.Registered);

        _userRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _userService.DeleteAsync(id, CancellationToken.None);

        _userRepositoryMock.Verify(r => r.Delete(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_FailNotFound_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();

        _userRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.DeleteAsync(id, CancellationToken.None));

        _userRepositoryMock.Verify(r => r.Delete(It.IsAny<User>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_SuccessPath_ReturnsMappedUser()
    {
        var id = Guid.NewGuid();
        var user = User.Create("username", "email@test.com", "hash", UserRole.Registered);
        var expectedResponse = new UserResponse { Id = id, Username = "username", Email = "email@test.com" };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mapperMock.Setup(m => m.Map<UserResponse>(user))
            .Returns(expectedResponse);

        var result = await _userService.GetByIdAsync(id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Id, result.Id);

        _userRepositoryMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_FailNotFound_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();

        _userRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.GetByIdAsync(id, CancellationToken.None));
    }

    [Fact]
    public async Task GetByEmailAsync_SuccessPath_ReturnsMappedUser()
    {
        var email = "email@test.com";
        var user = User.Create("username", email, "hash", UserRole.Registered);
        var expectedResponse = new UserResponse { Id = Guid.NewGuid(), Username = "username", Email = email };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mapperMock.Setup(m => m.Map<UserResponse>(user))
            .Returns(expectedResponse);

        var result = await _userService.GetByEmailAsync(email, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Email, result.Email);

        _userRepositoryMock.Verify(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsync_FailNotFound_ThrowsKeyNotFoundException()
    {
        var email = "notfound@test.com";

        _userRepositoryMock.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.GetByEmailAsync(email, CancellationToken.None));
    }

    [Fact]
    public async Task GetAllAsync_SuccessPath_ReturnsMappedCollection()
    {
        var users = new List<User>
        {
            User.Create("user1", "user1@test.com", "hash", UserRole.Registered),
            User.Create("user2", "user2@test.com", "hash", UserRole.Registered)
        };
        var expectedResponses = new List<UserResponse>
        {
            new UserResponse { Id = Guid.NewGuid(), Username = "user1" },
            new UserResponse { Id = Guid.NewGuid(), Username = "user2" }
        };

        _userRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);
        _mapperMock.Setup(m => m.Map<IEnumerable<UserResponse>>(users))
            .Returns(expectedResponses);

        var result = await _userService.GetAllAsync(CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedResponses.Count, result.Count());

        _userRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}