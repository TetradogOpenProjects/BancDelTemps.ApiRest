using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BancDelTemps.ApiRest.Models
{
    [Index(nameof(Email), IsUnique = true, Name = nameof(Email) + "_uniqueContraint")]
    public class User
    {
        public const int INITTIME = 10 * 60;//10 horas
        public static DateTime DefaultExpireTokenDate { get; set; } = DateTime.UtcNow.AddDays(1);
        public User() {
            Permisos = new List<UserPermiso>();
            Granted = new List<UserPermiso>();
            Revoked = new List<UserPermiso>();
            Validated = new List<User>();
        
        }
        public User([NotNull] ClaimsPrincipal principal) : this()
        {

            Claim[] claims = principal.Identities.FirstOrDefault().Claims.ToArray();

            IdExterno = claims[0].Value;
            Email = claims[^1].Value;
            Name = claims[2].Value;
            Surname = claims[3].Value;
            JoinDate = DateTime.UtcNow;
        }

        public string IdExterno { get; set; }

        public int Id { get; set; }
        public DateTime JoinDate { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        [Required]
        public string Email { get; set; }

        public int? ValidatorId { get; set; }
        public User Validator { get; set; }
        [NotMapped]
        public bool IsValidated => ValidatorId.HasValue;
        public ICollection<User> Validated { get; set; }
        public DateTime? StartHolidays { get; set; }
        public DateTime? EndHolidays { get; set; }
        [NotMapped]
        public bool IsOnHolidays => StartHolidays.HasValue && DateTime.UtcNow > StartHolidays.Value && (!EndHolidays.HasValue || DateTime.UtcNow < EndHolidays.Value);

        public ICollection<UserPermiso> Permisos { get; set; }

        [NotMapped]
        public IEnumerable<UserPermiso> PermisosActivos => Permisos.Where(p => p.IsActive);
        [NotMapped]
        public IEnumerable<string> PermisosActivosName => PermisosActivos.Select(p => p.Permiso.Nombre);
        public ICollection<UserPermiso> Granted { get; set; }
        public ICollection<UserPermiso> Revoked { get; set; }
        [NotMapped]
        public bool IsAdmin => PermisosActivosName.Any(p => Permiso.ADMIN.Equals(p));

        public ICollection<Transaccion> TransaccionesFrom { get; set; }
        public ICollection<Transaccion> TransaccionesIn { get; set; }
        public ICollection<Transaccion> TransaccionesSigned { get; set; }
        public ICollection<Transaccion> TransaccionesValidator { get; set; }

        [NotMapped]
        public int TotalMinutos =>INITTIME + TransaccionesIn.Sum(t => t.Minutos) - TransaccionesFrom.Sum(t => t.Minutos);



        public override string ToString()
        {
            return Email;
        }
        public JwtSecurityToken GetToken(IConfiguration configuration, DateTime expiraToken = default(DateTime))
        {
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            SigningCredentials signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            Claim[] claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,configuration["Jwt:Subject"]),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,DateTime.UtcNow.ToString()), 
                new Claim(nameof(Name),Name),
                new Claim(nameof(Email),Email),
                new Claim(nameof(JoinDate),JoinDate.ToString()),
                new Claim(nameof(IsValidated),IsValidated.ToString())

            };
            return new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"],
                                        claims, expires: Equals(expiraToken, default(DateTime)) ? DefaultExpireTokenDate : expiraToken,
                                        signingCredentials: signIn);
        }
        public static string GetEmailFromHttpContext(HttpContext context)
        {
            const int EMAIL = 4;//si cambio el orden de los claim en nameof(GetToken) tengo que mirar donde queda el Email de nuevo!!
            Claim[] claims = context.User.Identities.FirstOrDefault().Claims.ToArray();

            return claims[EMAIL].Value;
        }

    }
    public class UserDTO
    {
        public UserDTO() { }
        public UserDTO([NotNull]User user)
        {
            Name = user.Name;
            Surname = user.Surname;
            IsValidated = user.IsValidated;
            IsOnHoliDays = user.IsOnHolidays;
            Email = user.Email;
            JoinDate = user.JoinDate;
            Permisos = user.PermisosActivosName;
        }
        public string Name { get; set; }
        public string Surname { get; set; }
        public bool IsValidated { get; set; }
        public bool IsOnHoliDays { get; set; }
        public string Email { get; set; }
        public DateTime JoinDate { get; set; }
        public IEnumerable<string> Permisos { get; set; }
    }
}
