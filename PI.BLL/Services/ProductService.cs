using AutoMapper;
using PI.BLL.DTOs.Catalog;
using PI.BLL.Interfaces;
using PI.DAL.Interfaces;
using PI.DAL.Models;
using PI.DAL.Entities.Catalog;

namespace PI.BLL.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Guid> CreateAsync(CreateProductRequest request)
    {
        var category = await _unitOfWork.Categories.GetByIDAsync(request.CategoryId);
        if (category == null)
            throw new KeyNotFoundException($"Category with ID {request.CategoryId} was not found.");

        var product = Product.Create(
            request.CategoryId,
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.ImageUrl);

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.CompleteAsync();

        return product.Id;
    }

    public async Task UpdateAsync(UpdateProductRequest request)
    {
        var product = await _unitOfWork.Products.GetByIDAsync(request.Id);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {request.Id} was not found.");

        if (product.CategoryId != request.CategoryId)
        {
            var category = await _unitOfWork.Categories.GetByIDAsync(request.CategoryId);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {request.CategoryId} was not found.");
        }

        product.Update(
            request.CategoryId,
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.ImageUrl);

        _unitOfWork.Products.Update(product);
        await _unitOfWork.CompleteAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIDAsync(id);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {id} was not found.");

        _unitOfWork.Products.Delete(product);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id)
    {
        var product = await _unitOfWork.Products.GetWithDetailsAsync(id);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {id} was not found.");

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<List<ProductResponse>> GetAllAsync()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        return _mapper.Map<List<ProductResponse>>(products);
    }

    public async Task<(IEnumerable<ProductResponse> Items, int TotalCount)> GetPagedAsync(ProductFilterParams filter)
    {
        var (items, totalCount) = await _unitOfWork.Products.GetFilteredPagedAsync(filter);

        var mappedItems = _mapper.Map<IEnumerable<ProductResponse>>(items);

        return (mappedItems, totalCount);
    }
}