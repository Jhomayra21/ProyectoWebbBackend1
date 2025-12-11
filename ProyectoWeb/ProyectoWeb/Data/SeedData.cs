using Microsoft.EntityFrameworkCore;
using ProyectoWeb.Models;
using System.Security.Cryptography;
using System.Text;

namespace ProyectoWeb.Data
{
    public static class SeedData
    {
        public static void EnsureSeedData(ApplicationDbContext context)
        {
            context.Database.Migrate();

            if (!context.Users.Any())
            {
                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    Email = "admin@local",
                    PasswordHash = HashPassword("Admin123"),
                    Role = Role.Admin
                };
                context.Users.Add(admin);
            }

            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Electrónica", Description = "Dispositivos y gadgets" },
                    new Category { Name = "Ropa", Description = "Ropa y accesorios" }
                );
            }

            if (!context.Products.Any())
            {
                var cat = context.Categories.FirstOrDefault();
                context.Products.AddRange(
                    new Product { Name = "Auriculares", Description = "Auriculares inalámbricos", Price = 59.99m, Stock = 10, CategoryId = cat?.Id ?? 1 },
                    new Product { Name = "Camiseta", Description = "Camiseta de algodón", Price = 19.99m, Stock = 50, CategoryId = cat?.Id ?? 2 }
                );
            }

            context.SaveChanges();
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
