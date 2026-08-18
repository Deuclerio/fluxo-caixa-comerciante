using Lancamentos.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lancamentos.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly JwtTokenService _tokens;

    public AuthController(JwtTokenService tokens)
    {
        _tokens = tokens;
    }

    /// <summary>Emite um JWT de demonstração para o comerciante.</summary>
    [AllowAnonymous]
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Token([FromBody] LoginRequest request)
    {
        var token = _tokens.Emitir(request.Usuario, request.Senha);
        if (token is null)
        {
            return Unauthorized(new { title = "Credenciais inválidas.", code = "CREDENCIAIS_INVALIDAS" });
        }

        return Ok(new TokenResponse(token, "Bearer", 28800));
    }
}

public sealed record LoginRequest(string Usuario, string Senha);
public sealed record TokenResponse(string AccessToken, string TokenType, int ExpiresIn);
