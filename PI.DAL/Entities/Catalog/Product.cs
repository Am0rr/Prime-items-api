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

    private Product(Guid categoryId, string name, string description, decimal price, int stockQuantity, string? imageUrl)
    {
        CategoryId = categoryId;
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        ImageUrl = imageUrl;
    }

    public static Product Create(Guid categoryId, string name, string description, decimal price, int stockQuantity, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be null or empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Product description cannot be null or empty.", nameof(description));
        if (price < 0)
            throw new ArgumentException("Price must be a positive value.", nameof(price));
        if (stockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative.", nameof(stockQuantity));

        return new Product(categoryId, name, description, price, stockQuantity, imageUrl);
    }

    public void ChangeCategory(Guid categoryId)
    {
        CategoryId = categoryId;
    }

    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be null or empty.", nameof(name));
        Name = name;
    }

    public void ChangeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Product description cannot be null or empty.", nameof(description));
        Description = description;
    }

    public void ChangePrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Price must be a positive value.", nameof(price));
        Price = price;
    }

    public void ChangeStockQuantity(int stockQuantity)
    {
        if (stockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative.", nameof(stockQuantity));
        StockQuantity = stockQuantity;
    }

    public void ChangeImageUrl(string? imageUrl)
    {
        ImageUrl = imageUrl;
    }
}