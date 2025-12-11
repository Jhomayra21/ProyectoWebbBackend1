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
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cats = await _context.Categories.ToListAsync();
            var dto = cats.Select(c => new CategoryReadDto { Id = c.Id, Name = c.Name, Description = c.Description });
            return Ok(dto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var c = await _context.Categories.FindAsync(id);
            if (c == null) return NotFound();
            return Ok(new CategoryReadDto { Id = c.Id, Name = c.Name, Description = c.Description });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CategoryCreateDto dto)
        {
            var cat = new Category { Name = dto.Name, Description = dto.Description };
            _context.Categories.Add(cat);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = cat.Id }, new CategoryReadDto { Id = cat.Id, Name = cat.Name, Description = cat.Description });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CategoryCreateDto dto)
        {
            var cat = await _context.Categories.FindAsync(id);
            if (cat == null) return NotFound();
            cat.Name = dto.Name;
            cat.Description = dto.Description;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cat = await _context.Categories.FindAsync(id);
            if (cat == null) return NotFound();
            _context.Categories.Remove(cat);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
