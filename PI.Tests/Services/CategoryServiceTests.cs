using AutoMapper;
using Moq;
using PI.BLL.DTOs.Catalog;
using PI.BLL.Services;
using PI.DAL.Entities.Catalog;
using PI.DAL.Interfaces;

namespace PI.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapperMock = new Mock<IMapper>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepositoryMock.Object);

        _serviceProviderMock
            .Setup(x => x.GetService(It.IsAny<Type>()))
            .Returns((object?)null);

        _categoryService = new CategoryService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _serviceProviderMock.Object);
    }

    [Fact]
    public async Task CreateAsync_SuccessPath_AddsCategoryAndSaves()
    {
        var request = new CreateCategoryRequest("New Category", "Description");
        var expectedResponse = new CategoryResponse { Id = Guid.NewGuid(), Name = request.Name, Description = request.Description };

        _categoryRepositoryMock.Setup(r => r.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mapperMock.Setup(m => m.Map<CategoryResponse>(It.IsAny<Category>()))
            .Returns(expectedResponse);

        var result = await _categoryService.CreateAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Name, result.Name);

        _categoryRepositoryMock.Verify(r => r.Add(It.Is<Category>(c => c.Name == request.Name && c.Description == request.Description)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ThrowsInvalidOperationException()
    {
        var request = new CreateCategoryRequest("Existing Category", "Description");

        _categoryRepositoryMock.Setup(r => r.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _categoryService.CreateAsync(request, CancellationToken.None));

        _categoryRepositoryMock.Verify(r => r.Add(It.IsAny<Category>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_SuccessPath_NameAndDescriptionUpdated_SavesChanges()
    {
        var id = Guid.NewGuid();
        var category = Category.Create("Old Name", "Old Description");
        var request = new UpdateCategoryRequest("New Name", "New Description");

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepositoryMock.Setup(r => r.ExistsByNameAsync(request.Name!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _categoryService.UpdateAsync(id, request, CancellationToken.None);

        Assert.Equal(request.Name, category.Name);
        Assert.Equal(request.Description, category.Description);

        _categoryRepositoryMock.Verify(r => r.ExistsByNameAsync(request.Name!, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_SuccessPath_NoNameChange_DoesNotCheckExistenceAndSaves()
    {
        var id = Guid.NewGuid();
        var category = Category.Create("Same Name", "Old Description");
        var request = new UpdateCategoryRequest("Same Name", "New Description");

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        await _categoryService.UpdateAsync(id, request, CancellationToken.None);

        Assert.Equal("Same Name", category.Name);
        Assert.Equal(request.Description, category.Description);

        _categoryRepositoryMock.Verify(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_FailNotFound_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        var request = new UpdateCategoryRequest("Name", "Description");

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category)null!);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _categoryService.UpdateAsync(id, request, CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_FailDuplicateName_ThrowsInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var category = Category.Create("Old Name", "Description");
        var request = new UpdateCategoryRequest("Existing Name", "Description");

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryRepositoryMock.Setup(r => r.ExistsByNameAsync(request.Name!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _categoryService.UpdateAsync(id, request, CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_SuccessPath_DeletesCategoryAndSaves()
    {
        var id = Guid.NewGuid();
        var category = Category.Create("Name", "Description");

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        await _categoryService.DeleteAsync(id, CancellationToken.None);

        _categoryRepositoryMock.Verify(r => r.Delete(category), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_FailNotFound_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category)null!);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _categoryService.DeleteAsync(id, CancellationToken.None));

        _categoryRepositoryMock.Verify(r => r.Delete(It.IsAny<Category>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_SuccessPath_ReturnsMappedCategory()
    {
        var id = Guid.NewGuid();
        var category = Category.Create("Name", "Description");
        var expectedResponse = new CategoryResponse { Id = id, Name = "Name", Description = "Description" };

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _mapperMock.Setup(m => m.Map<CategoryResponse>(category))
            .Returns(expectedResponse);

        var result = await _categoryService.GetByIdAsync(id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Id, result.Id);

        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_FailNotFound_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category)null!);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _categoryService.GetByIdAsync(id, CancellationToken.None));
    }

    [Fact]
    public async Task GetAllAsync_SuccessPath_ReturnsMappedCollection()
    {
        var categories = new List<Category>
        {
            Category.Create("Category 1", "Desc 1"),
            Category.Create("Category 2", "Desc 2")
        };
        var expectedResponses = new List<CategoryResponse>
        {
            new CategoryResponse { Id = Guid.NewGuid(), Name = "Category 1", Description = "Desc 1" },
            new CategoryResponse { Id = Guid.NewGuid(), Name = "Category 2", Description = "Desc 2" }
        };

        _categoryRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        _mapperMock.Setup(m => m.Map<IEnumerable<CategoryResponse>>(categories))
            .Returns(expectedResponses);

        var result = await _categoryService.GetAllAsync(CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedResponses.Count, new List<CategoryResponse>(result).Count);

        _categoryRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}