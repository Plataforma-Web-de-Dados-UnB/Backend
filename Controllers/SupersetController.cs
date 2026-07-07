using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using api.Services.Interfaces;
using api.Views;

namespace api.Controllers;

[ApiController]
[Route("[controller]")]
public class SupersetController(ISupersetService supersetService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("guest-token")]
    public async Task<ActionResult<SupersetGuestTokenResponseDto>> GetGuestToken([FromBody] SupersetGuestTokenRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DashboardId))
            return BadRequest(new { message = "O ID do dashboard é obrigatório." });

        var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
        var username = isAuthenticated
            ? (User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.Email) ?? "usuario")
            : "visitante";
        var firstName = isAuthenticated
            ? (User.FindFirstValue(ClaimTypes.GivenName) ?? username)
            : "Visitante";
        var lastName = isAuthenticated
            ? (User.FindFirstValue(ClaimTypes.Surname) ?? "")
            : "UnB Portal";

        var resultado = await supersetService.GetGuestTokenAsync(dto.DashboardId, username, firstName, lastName).ConfigureAwait(false);

        if (!resultado.Success)
            return BadRequest(new { message = resultado.Error });

        return Ok(new SupersetGuestTokenResponseDto { Token = resultado.Data! });
    }
}
