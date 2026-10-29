using System.ComponentModel.DataAnnotations;

namespace RoleAuth.Api.Dtos;

public record RoleDto(int Id, string Name);

public class RoleRequest
{
    [Required, StringLength(50, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
}
