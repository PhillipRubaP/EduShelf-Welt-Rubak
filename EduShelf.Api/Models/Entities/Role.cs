using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduShelf.Api.Models.Entities;

public class Role
{
    [Key]
    public int RoleId { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}