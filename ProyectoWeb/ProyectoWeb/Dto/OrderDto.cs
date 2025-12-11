using System.ComponentModel.DataAnnotations;

namespace ProyectoWeb.Dto
{
    public class OrderCreateDto
    {
    }

    public class OrderReadDto
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class PaymentDto
    {
        [Required]
        public int OrderId { get; set; }

        public string? Method { get; set; }
    }
}
