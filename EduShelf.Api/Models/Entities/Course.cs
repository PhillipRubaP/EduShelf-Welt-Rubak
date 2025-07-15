using System.ComponentModel.DataAnnotations;

namespace EduShelf.Api.Models.Entities;

public class Course
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; }
}