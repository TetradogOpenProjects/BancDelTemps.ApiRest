using System.IdentityModel.Tokens.Jwt;

namespace BancDelTemps.ApiRest.Models
{
    public class LoginDTO
    {
        public LoginDTO(JwtSecurityToken token) => Token = token.WriteToken();
        public string Token { get; set; }
    }
}
