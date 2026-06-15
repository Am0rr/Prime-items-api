using PI.DAL.Entities.Catalog;

namespace PI.DAL.Entities.Orders;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public Order Order { get; private set; } = null!;
    public Product Product { get; private set; } = null!;

    protected OrderItem() { }

    public OrderItem(Guid orderId, Guid productId, int quantity, decimal unitPrice)
    {
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}