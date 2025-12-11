using System.ComponentModel.DataAnnotations;

namespace ProyectoWeb.Models
{
    public enum Role
    {
        Admin,
        Customer
    }

    public class User
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public Role Role { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
