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

    public async Task<Guid> CreateAsync(CreateCategoryRequest request)
    {
        var category = Category.Create(request.Name, request.Description);

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return category.Id;
    }

    public async Task UpdateAsync(UpdateCategoryRequest request)
    {
        var category = await _unitOfWork.Categories.GetByIDAsync(request.Id)
            ?? throw new KeyNotFoundException($"Category with ID {request.Id} was not found.");

        bool hasChanges = false;

        if (request.Name != category.Name)
        {
            category.UpdateName(request.Name);
            hasChanges = true;
        }

        if (request.Description != category.Description)
        {
            category.UpdateDescription(request.Description);
            hasChanges = true;
        }

        if (!hasChanges) return;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _unitOfWork.Categories.GetByIDAsync(id)
            ?? throw new KeyNotFoundException($"Category with ID {id} was not found.");

        _unitOfWork.Categories.Delete(category);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<CategoryResponse?> GetByIdAsync(Guid id)
    {
        var category = await _unitOfWork.Categories.GetByIDAsync(id)
            ?? throw new KeyNotFoundException($"Category with ID {id} was not found.");

        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task<List<CategoryResponse>> GetAllAsync()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return _mapper.Map<List<CategoryResponse>>(categories);
    }
}