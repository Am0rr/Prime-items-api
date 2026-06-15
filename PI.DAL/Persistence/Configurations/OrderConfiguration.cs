using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PI.DAL.Entities.Orders;

namespace PI.DAL.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.CreatedAt).IsRequired();
        
        builder.Property(o => o.Status)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(20);
               
        builder.Property(o => o.TotalAmount)
               .IsRequired()
               .HasColumnType("decimal(18,2)");

        builder.HasOne(o => o.User) 
               .WithMany() 
               .HasForeignKey(o => o.UserId)
               .IsRequired();

        builder.HasMany(o => o.OrderItems) 
               .WithOne(oi => oi.Order)
               .HasForeignKey(oi => oi.OrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}