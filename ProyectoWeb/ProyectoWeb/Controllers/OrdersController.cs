using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoWeb.Data;
using ProyectoWeb.Dto;
using ProyectoWeb.Models;
using System.Security.Claims;

namespace ProyectoWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        /// <summary>
        /// </summary>
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserId();
            var items = await _context.CartItems.Include(ci => ci.Product).Where(ci => ci.UserId == userId).ToListAsync();
            if (!items.Any()) return BadRequest(new { error = "Carrito vacío" });

            foreach (var it in items)
            {
                if (it.Quantity > it.Product.Stock) return BadRequest(new { error = $"Stock insuficiente para {it.Product.Name}" });
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order { UserId = userId, TotalAmount = items.Sum(i => i.Quantity * i.Product.Price), Status = OrderStatus.Pending };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                var orderItems = new List<OrderItem>();
                foreach (var it in items)
                {
                    orderItems.Add(new OrderItem { OrderId = order.Id, ProductId = it.ProductId, Quantity = it.Quantity, UnitPrice = it.Product.Price });
                    it.Product.Stock -= it.Quantity;
                }

                _context.OrderItems.AddRange(orderItems);
                _context.CartItems.RemoveRange(items);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                var read = new OrderReadDto
                {
                    Id = order.Id,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status.ToString(),
                    CreatedAt = order.CreatedAt,
                    Items = orderItems.Select(oi => new OrderItemDto { ProductId = oi.ProductId, ProductName = _context.Products.Find(oi.ProductId).Name, Quantity = oi.Quantity, UnitPrice = oi.UnitPrice }).ToList()
                };

                return Ok(read);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// </summary>
        [HttpPost("pay")]
        public async Task<IActionResult> Pay(PaymentDto dto)
        {
            var userId = GetUserId();
            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null) return NotFound(new { error = "Orden no encontrada" });
            if (order.UserId != userId && !User.IsInRole("Admin")) 
                return Forbid();
            if (order.Status != OrderStatus.Pending) 
                return BadRequest(new { error = "Orden no está pendiente de pago" });

            order.Status = OrderStatus.Paid;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Pago simulado exitoso", orderId = order.Id, status = "Paid" });
        }

        /// <summary>
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (User.IsInRole("Admin"))
            {
                var orders = await _context.Orders.Include(o => o.Items).ToListAsync();
                var dto = orders.Select(o => new OrderReadDto { Id = o.Id, TotalAmount = o.TotalAmount, Status = o.Status.ToString(), CreatedAt = o.CreatedAt, Items = o.Items.Select(i => new OrderItemDto { ProductId = i.ProductId, ProductName = _context.Products.Find(i.ProductId).Name, Quantity = i.Quantity, UnitPrice = i.UnitPrice }).ToList() });
                return Ok(dto);
            }
            else
            {
                var userId = GetUserId();
                var orders = await _context.Orders.Where(o => o.UserId == userId).Include(o => o.Items).ToListAsync();
                var dto = orders.Select(o => new OrderReadDto { Id = o.Id, TotalAmount = o.TotalAmount, Status = o.Status.ToString(), CreatedAt = o.CreatedAt, Items = o.Items.Select(i => new OrderItemDto { ProductId = i.ProductId, ProductName = _context.Products.Find(i.ProductId).Name, Quantity = i.Quantity, UnitPrice = i.UnitPrice }).ToList() });
                return Ok(dto);
            }
        }

        /// <summary>
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound(new { error = "Orden no encontrada" });
            if (!User.IsInRole("Admin") && order.UserId != GetUserId()) 
                return Forbid();

            var dto = new OrderReadDto { Id = order.Id, TotalAmount = order.TotalAmount, Status = order.Status.ToString(), CreatedAt = order.CreatedAt, Items = order.Items.Select(i => new OrderItemDto { ProductId = i.ProductId, ProductName = _context.Products.Find(i.ProductId).Name, Quantity = i.Quantity, UnitPrice = i.UnitPrice }).ToList() };
            return Ok(dto);
        }
    }
}
