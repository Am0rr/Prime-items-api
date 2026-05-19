namespace PI.DAL.Entities.Catalog;

public class Category : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    public ICollection<Product> Products { get; private set; } = null!;

    protected Category()
    {
        Products = new List<Product>();
    }

    private Category(string name, string description) : this()
    {
        Name = name;
        Description = description;
    }

    public static Category Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be null or empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Category description cannot be null or empty.", nameof(description));

        return new Category(name, description);
    }

    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be null or empty.", nameof(name));
        Name = name;
    }

    public void ChangeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Category description cannot be null or empty.", nameof(description));
        Description = description;
    }
}