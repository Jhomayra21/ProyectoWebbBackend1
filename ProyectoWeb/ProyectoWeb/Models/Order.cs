using System.ComponentModel.DataAnnotations;

namespace ProyectoWeb.Models
{
    public enum OrderStatus
    {
        Pending,
        Paid,
        Cancelled
    }

    public class Order
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public ICollection<OrderItem> Items { get; set; }
    }
}
