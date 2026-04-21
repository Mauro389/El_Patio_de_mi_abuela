using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Negocio
{
    public class JwtHelper
    {
        private readonly IConfiguration _configuration;

        public JwtHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerarToken(int idUsuario, string username, int idRol, string nombreCompleto)
        {
            var jwtConfig = _configuration.GetSection("JwtConfig");
            var secretKey = jwtConfig["SecretKey"];
            var issuer = jwtConfig["Issuer"];
            var audience = jwtConfig["Audience"];
            var expirationInMinutes = Convert.ToInt32(jwtConfig["ExpirationInMinutes"]);

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey no configurada en appsettings.json");
            }

            
            var claims = new[]
            {
                new Claim("IdUsuario", idUsuario.ToString()),   
                new Claim(ClaimTypes.Name, nombreCompleto),    
                new Claim("role", idRol.ToString()),            
                new Claim("Username", username)                 
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(expirationInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}