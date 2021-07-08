using BancDelTemps.ApiRest.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;
using Microsoft.EntityFrameworkCore.InMemory;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using Microsoft.Extensions.Primitives;

namespace BancDelTemps.ApiRest.Testing
{
    public class MockHttpContext : IHttpContext
    {
        public bool IsAuthenticated { get; set; }
        public string[] Claims { get; set; }
        public string[] GetClaimsValueFirstIdentity() => Claims;
    }
    public class Configuration : IConfiguration
    {
        public Configuration()
        {
            DicConfig = new SortedList<string, string>();

            this["Jwt:Key"] = "passwordSeguraQuinceVentiUnoTreinta123";
            this["Jwt:Issuer"] = "InventoryAuthenticationServer";
            this["Jwt:Audience"] = "InventoryServicePostmanClient";
            this["Jwt:Subject"] = "InventoryServiceAccessToken";

        }
        public SortedList<string, string> DicConfig { get; set; }
        public string this[string key]
        {
            get => DicConfig[key];
            set
            {
                if (DicConfig.ContainsKey(key))
                    DicConfig.Remove(key);
                DicConfig.Add(key, value);
            }
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            throw new NotImplementedException();
        }

        public IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
        }

        public IConfigurationSection GetSection(string key)
        {
            throw new NotImplementedException();
        }
    }
    public delegate void TestUserMethod(User user);
    public abstract class TestBase
    {
        public TestBase()
        {
            DbContextOptionsBuilder<Context> optionsBilder = new DbContextOptionsBuilder<Context>();
            optionsBilder.UseInMemoryDatabase("bdTesting");
            Context = new Context(optionsBilder.Options);
            for (int i = 0; i < Permiso.Todos.Length; i++)
            {
                Context.Permisos.Add(new Permiso() { Nombre = Permiso.Todos[i] });
            }
            Context.SaveChanges();
            Configuration = new Configuration();
            ContextoHttp = new MockHttpContext();

        }

        public MockHttpContext ContextoHttp { get; private set; }
        public Configuration Configuration { get; private set; }
        public Context Context { get; private set; }

        public User GetNoValidatedUser()
        {
            User userNotValidated = Context.Users.Where(u => !u.IsValidated).FirstOrDefault();
            if (Equals(userNotValidated, default))
            {
                userNotValidated = new User()
                {
                    Name = "Invalidated",
                    Email = "Invalidated@email",
                    JoinDate = DateTime.UtcNow
                };
                Context.Users.Add(userNotValidated);
                Context.SaveChanges();
            }
            return userNotValidated;
        }
        public User GetValidatedUser()
        {
            User userValidated = Context.Users.Include(u => u.Permisos).Where(u => u.IsValidated && u.Permisos.Where(p=>p.IsActive).Count() == 0).FirstOrDefault();
            User userValidator = Context.Users.Include(u => u.Permisos).ThenInclude(p => p.Permiso).Where(u => u.IsValidated && u.IsModValidation).FirstOrDefault();
            if (Equals(userValidated, default))
            {
                if (Equals(userValidator, default))
                {
                    userValidator = GetUserWithPermiso(Permiso.MODVALIDATION);
                }
                userValidated = new User()
                {
                    Name = "Validated",
                    Email = "Validated@email",
                    JoinDate = DateTime.UtcNow,
                    Validator = userValidator
                };
                Context.Users.Add(userValidated);
                Context.SaveChanges();
            }
            return userValidated;
        }
        public User GetUserWithPermiso(string permiso)
        {
            User superUser = Context.Users.Include(u => u.Permisos).ThenInclude(p=>p.Permiso).Where(u => u.IsValidated && u.Permisos.Any(p=>p.IsActive && p.Permiso.Nombre.Equals(permiso))).FirstOrDefault();
            if (Equals(superUser, default))
            {

                superUser = new User()
                {
                    Name = $"User{permiso}",
                    Email = $"User{permiso.Trim(' ')}@email",
                    JoinDate = DateTime.UtcNow
                };
                superUser.Validator = superUser;
                Context.Users.Add(superUser);
                Context.PermisosUsuarios.Add(new UserPermiso() { Permiso = Context.Permisos.Find(Permiso.MODVALIDATION), GrantedBy = superUser, GrantedDate = DateTime.UtcNow, User = superUser });
                Context.SaveChanges();
            }
            return superUser;
        }
        public IEnumerable<User> GetUserPermisos() => Permiso.Todos.Select(p => GetUserWithPermiso(p));

        public void DoAction(User user, TestUserMethod method)
        {
         
            try
            {
   
                ContextoHttp.IsAuthenticated = !Equals(user, default);
                if (ContextoHttp.IsAuthenticated)
                    ContextoHttp.Claims = new string[] { string.Empty, string.Empty, string.Empty, string.Empty, user.Email };
                method(user);
            }
            catch { throw; }



        }
    }
}
