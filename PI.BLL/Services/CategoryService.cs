using AutoMapper;
using PI.BLL.DTOs.Catalog;
using PI.BLL.Interfaces;
using PI.DAL.Interfaces;
using PI.DAL.Entities.Catalog;

namespace PI.BLL.Services;

public class CategoryService : BaseService, ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(request);

        var category = Category.Create(request.Name, request.Description);

        _unitOfWork.Categories.Add(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(request);

        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with ID {id} was not found.");

        if (request.Name != null && request.Name != category.Name)
        {
            if (await _unitOfWork.Categories.ExistsByNameAsync(request.Name, cancellationToken))
                throw new InvalidOperationException($"A category with the name '{request.Name}' already exists.");

            category.ChangeName(request.Name);
        }

        if (request.Description != null && request.Description != category.Description)
        {
            category.ChangeDescription(request.Description);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with ID {id} was not found.");

        _unitOfWork.Categories.Delete(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CategoryResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with ID {id} was not found.");

        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);

        return _mapper.Map<IEnumerable<CategoryResponse>>(categories);
    }
}