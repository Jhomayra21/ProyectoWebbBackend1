using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoWeb.Data;
using ProyectoWeb.Dto;
using ProyectoWeb.Models;

namespace ProyectoWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? categoryId, [FromQuery] string? search)
        {
            var q = _context.Products.Include(p => p.Category).AsQueryable();
            if (categoryId.HasValue) q = q.Where(p => p.CategoryId == categoryId.Value);
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(p => p.Name.Contains(search));

            var products = await q.ToListAsync();
            var dto = products.Select(p => new ProductReadDto { Id = p.Id, Name = p.Name, Description = p.Description, Price = p.Price, Stock = p.Stock, CategoryId = p.CategoryId, CategoryName = p.Category?.Name });
            return Ok(dto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var p = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();
            return Ok(new ProductReadDto { Id = p.Id, Name = p.Name, Description = p.Description, Price = p.Price, Stock = p.Stock, CategoryId = p.CategoryId, CategoryName = p.Category?.Name });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateDto dto)
        {
            var exists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!exists) return BadRequest(new { error = "Categoría no existe" });

            var p = new Product { Name = dto.Name, Description = dto.Description, Price = dto.Price, Stock = dto.Stock, CategoryId = dto.CategoryId };
            _context.Products.Add(p);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = p.Id }, new ProductReadDto { Id = p.Id, Name = p.Name, Description = p.Description, Price = p.Price, Stock = p.Stock, CategoryId = p.CategoryId });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProductCreateDto dto)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return NotFound();
            p.Name = dto.Name; p.Description = dto.Description; p.Price = dto.Price; p.Stock = dto.Stock; p.CategoryId = dto.CategoryId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return NotFound();
            _context.Products.Remove(p);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
