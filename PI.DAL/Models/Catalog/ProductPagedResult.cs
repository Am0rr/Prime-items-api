using PI.DAL.Entities.Catalog;

namespace PI.DAL.Models.Catalog;

public class ProductPagedResult
{
    public IEnumerable<Product> Items { get; set; } = new List<Product>();
    public int TotalCount { get; set; }
}