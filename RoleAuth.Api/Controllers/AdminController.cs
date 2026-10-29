using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoleAuth.Api.Models;
using RoleAuth.Api.Services;

namespace RoleAuth.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = RoleNames.Admin)]
public class AdminController(IBirthdayService birthdayService) : ControllerBase
{
    /// <summary>Запускає розсилку привітань вручну (для тестування; ігнорує годину SendAtHour).</summary>
    [HttpPost("birthdays/run")]
    public async Task<IActionResult> RunBirthdays(CancellationToken ct)
    {
        var sent = await birthdayService.SendDueGreetingsAsync(ct);
        return Ok(new { sent });
    }
}
