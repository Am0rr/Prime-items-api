using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PI.DAL.Entities.Orders;

namespace PI.DAL.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.CreatedAt).IsRequired();

        builder.Property(oi => oi.UnitPrice)
               .IsRequired()
               .HasColumnType("decimal(18,2)");

        builder.Property(oi => oi.Quantity)
               .IsRequired();

        builder.HasOne(oi => oi.Product)
               .WithMany() 
               .HasForeignKey(oi => oi.ProductId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict); 
    }
}