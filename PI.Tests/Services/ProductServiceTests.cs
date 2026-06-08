using AutoMapper;
using Moq;
using PI.BLL.DTOs.Catalog;
using PI.BLL.Services;
using PI.DAL.Entities.Catalog;
using PI.DAL.Interfaces;
using PI.DAL.Models.Catalog;

namespace PI.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _mapperMock = new Mock<IMapper>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepositoryMock.Object);

        _serviceProviderMock
            .Setup(x => x.GetService(It.IsAny<Type>()))
            .Returns((object?)null);

        _productService = new ProductService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _serviceProviderMock.Object);
    }

    [Fact]
    public async Task CreateAsync_SuccessPath_AddsProductAndSaves()
    {
        var categoryId = Guid.NewGuid();
        var request = new CreateProductRequest(categoryId, "Gaming Chair", "Ergonomic chair", 299.99m, 15, "image.png");
        var category = Category.Create("Furniture", "Office and Gaming Furniture");
        var expectedResponse = new ProductResponse { Id = Guid.NewGuid(), Name = request.Name, CategoryId = categoryId };

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _mapperMock.Setup(m => m.Map<ProductResponse>(It.IsAny<Product>()))
            .Returns(expectedResponse);

        var result = await _productService.CreateAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Name, result.Name);

        _productRepositoryMock.Verify(r => r.ExistsByNameAsync(request.Name, It.IsAny<CancellationToken>()), Times.Once);
        _productRepositoryMock.Verify(r => r.Add(It.Is<Product>(p => p.Name == request.Name && p.CategoryId == request.CategoryId)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CategoryNotFound_ThrowsKeyNotFoundException()
    {
        var request = new CreateProductRequest(Guid.NewGuid(), "Gaming Chair", "Ergonomic chair", 299.99m, 15, "image.png");

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category)null!);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _productService.CreateAsync(request, CancellationToken.None));

        _productRepositoryMock.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_SuccessPath_FullUpdate_TriggersChangeMethodsAndSaves()
    {
        var productId = Guid.NewGuid();
        var oldCategoryId = Guid.NewGuid();
        var newCategoryId = Guid.NewGuid();
        var product = Product.Create(oldCategoryId, "Old Name", "Old Desc", 100m, 5, "old.png");
        var request = new UpdateProductRequest(newCategoryId, "New Name", "New Desc", 150m, 10, "new.png");
        var validCategory = Category.Create("New Category", "Category Desc");

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(newCategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validCategory);
        _productRepositoryMock.Setup(r => r.ExistsByNameAsync(request.Name!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _productService.UpdateAsync(productId, request, CancellationToken.None);

        Assert.Equal(newCategoryId, product.CategoryId);
        Assert.Equal(request.Name, product.Name);
        Assert.Equal(request.Description, product.Description);
        Assert.Equal(request.Price, product.Price);
        Assert.Equal(request.StockQuantity, product.StockQuantity);
        Assert.Equal(request.ImageUrl, product.ImageUrl);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_SuccessPath_NoTriggerForChecks_DoesNotCallDatabaseLookups()
    {
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create(categoryId, "Same Name", "Same Desc", 100m, 5, "same.png");
        var request = new UpdateProductRequest(categoryId, "Same Name", "Same Desc", 100m, 5, "same.png");

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        await _productService.UpdateAsync(productId, request, CancellationToken.None);

        _categoryRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _productRepositoryMock.Verify(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_FailProductNotFound_ThrowsKeyNotFoundException()
    {
        var productId = Guid.NewGuid();
        var request = new UpdateProductRequest(null, "Name", null, null, null, null);

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null!);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _productService.UpdateAsync(productId, request, CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_FailCategoryNotFound_ThrowsKeyNotFoundException()
    {
        var productId = Guid.NewGuid();
        var oldCategoryId = Guid.NewGuid();
        var newCategoryId = Guid.NewGuid();
        var product = Product.Create(oldCategoryId, "Name", "Desc", 100m, 5, "url.png");
        var request = new UpdateProductRequest(newCategoryId, null, null, null, null, null);

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(newCategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category)null!);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _productService.UpdateAsync(productId, request, CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_FailDuplicateName_ThrowsInvalidOperationException()
    {
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var product = Product.Create(categoryId, "Old Name", "Desc", 100m, 5, "url.png");
        var request = new UpdateProductRequest(null, "Existing Name", null, null, null, null);

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _productRepositoryMock.Setup(r => r.ExistsByNameAsync(request.Name!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _productService.UpdateAsync(productId, request, CancellationToken.None));

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_SuccessPath_DeletesProductAndSaves()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create(Guid.NewGuid(), "Name", "Desc", 10m, 5, null);

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        await _productService.DeleteAsync(productId, CancellationToken.None);

        _productRepositoryMock.Verify(r => r.Delete(product), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_FailNotFound_ThrowsKeyNotFoundException()
    {
        var productId = Guid.NewGuid();

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null!);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _productService.DeleteAsync(productId, CancellationToken.None));

        _productRepositoryMock.Verify(r => r.Delete(It.IsAny<Product>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_SuccessPath_ReturnsMappedProduct()
    {
        var productId = Guid.NewGuid();
        var product = Product.Create(Guid.NewGuid(), "Name", "Desc", 10m, 5, null);
        var expectedResponse = new ProductResponse { Id = productId, Name = "Name" };

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mapperMock.Setup(m => m.Map<ProductResponse>(product))
            .Returns(expectedResponse);

        var result = await _productService.GetByIdAsync(productId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_FailNotFound_ThrowsKeyNotFoundException()
    {
        var productId = Guid.NewGuid();

        _productRepositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product)null!);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _productService.GetByIdAsync(productId, CancellationToken.None));
    }

    [Fact]
    public async Task GetAllAsync_SuccessPath_ReturnsMappedCollection()
    {
        var products = new List<Product>
        {
            Product.Create(Guid.NewGuid(), "P1", "D1", 10m, 2, null),
            Product.Create(Guid.NewGuid(), "P2", "D2", 20m, 4, null)
        };
        var expectedResponses = new List<ProductResponse>
        {
            new ProductResponse { Name = "P1" },
            new ProductResponse { Name = "P2" }
        };

        _productRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
        _mapperMock.Setup(m => m.Map<IEnumerable<ProductResponse>>(products))
            .Returns(expectedResponses);

        var result = await _productService.GetAllAsync(CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedResponses.Count, new List<ProductResponse>(result).Count);
    }

    [Fact]
    public async Task GetPagedAsync_SuccessPath_ReturnsCorrectProductPagedResponse()
    {
        var filter = new ProductFilterModel { SearchTerm = "Chair", PageNumber = 1, PageSize = 10 };
        var products = new List<Product> { Product.Create(Guid.NewGuid(), "Chair", "Comfortable", 150m, 5, null) };
        var pagedResult = new ProductPagedResult { Items = products, TotalCount = 1 };

        var mappedItems = new List<ProductResponse> { new ProductResponse { Name = "Chair", Price = 150m } };

        _productRepositoryMock.Setup(r => r.GetFilteredPagedAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);
        _mapperMock.Setup(m => m.Map<IEnumerable<ProductResponse>>(pagedResult.Items))
            .Returns(mappedItems);

        var result = await _productService.GetPagedAsync(filter, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(pagedResult.TotalCount, result.TotalCount);
        Assert.Equal(mappedItems, result.Items);

        _productRepositoryMock.Verify(r => r.GetFilteredPagedAsync(filter, It.IsAny<CancellationToken>()), Times.Once);
    }
}