using HRM.Business.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HRM.Business.Helpers
{
    public class JwtHelper
    {
        private readonly IConfiguration _configuration;

        public JwtHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(CurrentUser currentUser)
        {
            if (currentUser == null)
            {
                throw new ArgumentNullException(nameof(currentUser));
            }

            string key = _configuration["Jwt:Key"] ?? string.Empty;
            string issuer = _configuration["Jwt:Issuer"] ?? string.Empty;
            string audience = _configuration["Jwt:Audience"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException("Jwt:Key is missing in appsettings.json.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, currentUser.UserId.ToString()),
                new Claim(ClaimTypes.Name, currentUser.Username),
                new Claim(ClaimTypes.Email, currentUser.Email ?? string.Empty),
                new Claim("UserId", currentUser.UserId.ToString())
            };

            if (currentUser.EmployeeId.HasValue)
            {
                claims.Add(new Claim("EmployeeId", currentUser.EmployeeId.Value.ToString()));
            }

            foreach (var role in currentUser.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            int expireMinutes = 120;

            string? expireConfig = _configuration["Jwt:ExpireMinutes"];

            if (!string.IsNullOrWhiteSpace(expireConfig) && int.TryParse(expireConfig, out int parsedExpireMinutes))
            {
                expireMinutes = parsedExpireMinutes;
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(expireMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}