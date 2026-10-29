using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoleAuth.Api.Dtos;
using RoleAuth.Api.Models;
using RoleAuth.Api.Services;

namespace RoleAuth.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = RoleNames.Admin)]
public class RolesController(IRoleService roleService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoleDto>>> GetAll(CancellationToken ct) =>
        Ok(await roleService.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoleDto>> GetById(int id, CancellationToken ct) =>
        Ok(await roleService.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<RoleDto>> Create([FromBody] RoleRequest request, CancellationToken ct)
    {
        var role = await roleService.CreateAsync(request.Name, ct);
        return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RoleDto>> Update(int id, [FromBody] RoleRequest request, CancellationToken ct) =>
        Ok(await roleService.UpdateAsync(id, request.Name, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await roleService.DeleteAsync(id, ct);
        return NoContent();
    }
}
