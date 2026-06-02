using AutoMapper;
using PI.BLL.DTOs.Catalog;
using PI.BLL.Interfaces;
using PI.DAL.Interfaces;
using PI.DAL.Entities.Catalog;
using PI.DAL.Models.Catalog;

namespace PI.BLL.Services;

public class ProductService : BaseService, IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(request);

        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with ID {request.CategoryId} was not found.");

        var product = Product.Create(
            request.CategoryId,
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.ImageUrl);

        _unitOfWork.Products.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(request);

        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product with ID {id} was not found.");

        if (request.CategoryId.HasValue && request.CategoryId.Value != product.CategoryId)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId.Value, cancellationToken)
                ?? throw new KeyNotFoundException($"Category with ID {request.CategoryId.Value} was not found.");

            product.ChangeCategory(request.CategoryId.Value);
        }

        if (request.Name != null && request.Name != product.Name)
        {
            if (await _unitOfWork.Products.ExistsByNameAsync(request.Name, cancellationToken))
                throw new InvalidOperationException($"A product with the name '{request.Name}' already exists.");

            product.ChangeName(request.Name);
        }

        if (request.Description != null && request.Description != product.Description)
        {
            product.ChangeDescription(request.Description);
        }

        if (request.Price.HasValue && request.Price.Value != product.Price)
        {
            product.ChangePrice(request.Price.Value);
        }

        if (request.StockQuantity.HasValue && request.StockQuantity.Value != product.StockQuantity)
        {
            product.ChangeStockQuantity(request.StockQuantity.Value);
        }

        if (request.ImageUrl != null && request.ImageUrl != product.ImageUrl)
        {
            product.ChangeImageUrl(request.ImageUrl);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product with ID {id} was not found.");

        _unitOfWork.Products.Delete(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Product with ID {id} was not found.");

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<IEnumerable<ProductResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetAllAsync(cancellationToken);

        return _mapper.Map<IEnumerable<ProductResponse>>(products);
    }

    public async Task<ProductPagedResponse> GetPagedAsync(ProductFilterModel filter, CancellationToken cancellationToken)
    {
        var result = await _unitOfWork.Products.GetFilteredPagedAsync(filter, cancellationToken);

        var mappedItems = _mapper.Map<IEnumerable<ProductResponse>>(result.Items);

        return new ProductPagedResponse(mappedItems, result.TotalCount);
    }
}