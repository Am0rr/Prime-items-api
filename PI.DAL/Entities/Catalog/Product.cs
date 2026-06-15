namespace PI.DAL.Entities.Catalog;

public class Product : BaseEntity
{
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public string? ImageUrl { get; private set; }
    public Category Category { get; private set; } = null!;

    protected Product() { }

    public Product(Guid categoryId, string name, string description, decimal price, int stockQuantity, string? imageUrl)
    {
        CategoryId = categoryId;
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        ImageUrl = imageUrl;
    }

    public void ChangeCategory(Guid categoryId) => CategoryId = categoryId;

    public void ChangeName(string name) => Name = name;

    public void ChangeDescription(string description) => Description = description;

    public void ChangePrice(decimal price) => Price = price;

    public void ChangeStockQuantity(int stockQuantity) => StockQuantity = stockQuantity;

    public void ChangeImageUrl(string? imageUrl) => ImageUrl = imageUrl;
}