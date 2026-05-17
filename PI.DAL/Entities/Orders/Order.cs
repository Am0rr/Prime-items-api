using PI.DAL.Entities.Identity;
using PI.DAL.Enums;

namespace PI.DAL.Entities.Orders;

public class Order : BaseEntity
{
    public Guid UserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public User User {get; private set; } = null!;
    public ICollection<OrderItem> OrderItems { get; private set; } = null!;
        
    protected Order()
    {
        OrderItems = new List<OrderItem>();
    } 

    private Order(Guid userId, decimal totalAmount, OrderStatus status = OrderStatus.New) : this()
    {
        UserId = userId;
        TotalAmount = totalAmount;
        Status = status;
    }

    public static Order Create(Guid userId, decimal totalAmount, OrderStatus status = OrderStatus.New)
    {
        if (totalAmount < 0)
            throw new ArgumentException("Total amount cannot be negative.", nameof(totalAmount));

        return new Order(userId, totalAmount, status);
    }
    
    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
    }
}