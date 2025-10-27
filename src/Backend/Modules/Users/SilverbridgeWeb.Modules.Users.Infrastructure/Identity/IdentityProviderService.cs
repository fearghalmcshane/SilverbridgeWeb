using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SilverbridgeWeb.Modules.Users.Application.Abstractions.Identity;

namespace SilverbridgeWeb.Modules.Users.Infrastructure.Identity;

internal sealed class IdentityProviderService(IConfiguration configuration) : IIdentityProviderService
{
    public string GenerateToken(Guid userId, string email, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Users:Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        DateTime expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(configuration["Users:Jwt:ExpiryInMinutes"], NumberFormatInfo.InvariantInfo));

        var token = new JwtSecurityToken(
            issuer: configuration["Users:Jwt:Issuer"],
            audience: configuration["Users:Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
