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
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        /// <summary>
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = GetUserId();
            var items = await _context.CartItems.Include(ci => ci.Product).Where(ci => ci.UserId == userId).ToListAsync();
            var dto = items.Select(i => new CartItemDto { Id = i.Id, ProductId = i.ProductId, ProductName = i.Product.Name, UnitPrice = i.Product.Price, Quantity = i.Quantity });
            return Ok(dto);
        }

        /// <summary>
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Add(AddToCartDto dto)
        {
            var userId = GetUserId();
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null) return BadRequest(new { error = "Producto no encontrado" });
            if (dto.Quantity <= 0) return BadRequest(new { error = "Cantidad debe ser mayor a 0" });

            var existing = await _context.CartItems.FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == dto.ProductId);
            if (existing != null)
            {
                existing.Quantity += dto.Quantity;
            }
            else
            {
                _context.CartItems.Add(new CartItem { UserId = userId, ProductId = dto.ProductId, Quantity = dto.Quantity });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Producto agregado al carrito" });
        }

        /// <summary>
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, AddToCartDto dto)
        {
            var userId = GetUserId();
            var item = await _context.CartItems.FindAsync(id);
            if (item == null || item.UserId != userId) return NotFound();
            if (dto.Quantity <= 0) return BadRequest(new { error = "Cantidad debe ser mayor a 0" });

            if (dto.ProductId != item.ProductId)
            {
                var newProduct = await _context.Products.FindAsync(dto.ProductId);
                if (newProduct == null) return BadRequest(new { error = "Producto destino no encontrado" });

                var existing = await _context.CartItems.FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == dto.ProductId);
                if (existing != null)
                {
                    existing.Quantity += dto.Quantity;
                    _context.CartItems.Remove(item);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Producto actualizado en el carrito (fusionado con item existente)" });
                }
                else
                {
                    item.ProductId = dto.ProductId;
                    item.Quantity = dto.Quantity;
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Producto y cantidad actualizados" });
                }
            }

            item.Quantity = dto.Quantity;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cantidad actualizada" });
        }

        /// <summary>
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var item = await _context.CartItems.FindAsync(id);
            if (item == null || item.UserId != userId) return NotFound();
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Producto removido del carrito" });
        }
    }
}
