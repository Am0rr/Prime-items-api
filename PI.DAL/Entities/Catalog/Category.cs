namespace PI.DAL.Entities.Catalog;

public class Category : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public ICollection<Product> Products { get; private set; } = null!;

    protected Category() { }

    public Category(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public void ChangeName(string name) => Name = name;

    public void ChangeDescription(string description) => Description = description;
}