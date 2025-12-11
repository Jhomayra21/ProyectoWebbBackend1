using System.ComponentModel.DataAnnotations;

namespace ProyectoWeb.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
