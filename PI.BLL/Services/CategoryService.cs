using AutoMapper;
using PI.BLL.DTOs.Catalog;
using PI.BLL.Interfaces;
using PI.DAL.Interfaces;
using PI.DAL.Entities.Catalog;

namespace PI.BLL.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Guid> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = Category.Create(request.Name, request.Description);

        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return category.Id;
    }

    public async Task UpdateAsync(UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with ID {request.Id} was not found.");

        bool hasChanges = false;

        if (request.Name != null && request.Name != category.Name)
        {
            if (await _unitOfWork.Categories.ExistsByNameAsync(request.Name, cancellationToken))
                throw new InvalidOperationException($"A category with the name '{request.Name}' already exists.");

            category.ChangeName(request.Name);
            hasChanges = true;
        }

        if (request.Description != null && request.Description != category.Description)
        {
            category.ChangeDescription(request.Description);
            hasChanges = true;
        }

        if (!hasChanges) return;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with ID {id} was not found.");

        _unitOfWork.Categories.Delete(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CategoryResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Category with ID {id} was not found.");

        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task<List<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
        return _mapper.Map<List<CategoryResponse>>(categories);
    }
}