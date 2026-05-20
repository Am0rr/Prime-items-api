using PI.DAL.Entities.Identity;
using PI.DAL.Enums;

namespace PI.DAL.Entities.Orders;

public class Order : BaseEntity
{
    public Guid UserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public User User { get; private set; } = null!;
    public ICollection<OrderItem> OrderItems { get; private set; } = null!;

    protected Order()
    {
        OrderItems = new List<OrderItem>();
    }

    private Order(Guid userId, OrderStatus status = OrderStatus.New) : this()
    {
        UserId = userId;
        Status = status;
        TotalAmount = 0;
    }

    public static Order Create(Guid userId)
    {
        return new Order(userId);
    }

    public void AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        var item = OrderItem.Create(this.Id, productId, quantity, unitPrice);
        OrderItems.Add(item);

        TotalAmount += quantity * unitPrice;
    }

    public void ChangeStatus(OrderStatus newStatus)
    {
        Status = newStatus;
    }
}