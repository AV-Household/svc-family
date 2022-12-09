using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AV.Household.WebServer.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AV.Household.Family.Specs.Drivers;

public class AuthFakerDriver
{
    private static readonly Jwt JwtOptions = new()
    {
        Issuer = "AVHousehold",
        SecurityKey = "Pa$$w0rd_Pa$$w0rd_Pa$$w0rd"
    };

    public string GetBearer(string email, int household, bool isAdult)
    {
        var subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimsIdentity.DefaultNameClaimType, email),
            new Claim("Household", household.ToString()),
            new Claim(ClaimsIdentity.DefaultRoleClaimType, isAdult ? "Adult" : "Child")
        });

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = JwtOptions.Issuer,
            Audience = $"*.{JwtOptions.Issuer}",
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(10),
            Subject = subject,
            SigningCredentials =
                new SigningCredentials(JwtOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}