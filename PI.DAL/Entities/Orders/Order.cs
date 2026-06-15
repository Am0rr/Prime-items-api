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

    protected Order() { }

    public Order(Guid userId, OrderStatus status = OrderStatus.New)
    {
        UserId = userId;
        Status = status;
        TotalAmount = 0;
    }

    public void AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        var item = new OrderItem(Id, productId, quantity, unitPrice);
        OrderItems.Add(item);

        TotalAmount += quantity * unitPrice;
    }

    public void ChangeStatus(OrderStatus newStatus) => Status = newStatus;
}