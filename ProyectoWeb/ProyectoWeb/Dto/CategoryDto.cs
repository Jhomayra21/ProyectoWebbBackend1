using System.ComponentModel.DataAnnotations;

namespace ProyectoWeb.Dto
{
    public class CategoryCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class CategoryReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
