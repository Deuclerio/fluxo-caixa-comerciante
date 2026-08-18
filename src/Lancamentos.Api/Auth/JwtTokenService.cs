using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Lancamentos.Api.Auth;

public sealed class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? Emitir(string usuario, string senha)
    {
        var esperadoUsuario = _configuration["Auth:Usuario"] ?? "comerciante";
        var esperadoSenha = _configuration["Auth:Senha"] ?? "Fluxo@2026";

        if (!string.Equals(usuario, esperadoUsuario, StringComparison.Ordinal) ||
            !string.Equals(senha, esperadoSenha, StringComparison.Ordinal))
        {
            return null;
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(8);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, usuario),
                new Claim(ClaimTypes.Name, usuario),
                new Claim(ClaimTypes.Role, "comerciante")
            ],
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
