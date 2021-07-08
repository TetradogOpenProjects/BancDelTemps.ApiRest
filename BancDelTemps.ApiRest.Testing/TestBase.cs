using BancDelTemps.ApiRest.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;
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
        const string DEFAULT = "server=localhost;port=3322;user id=root;password=admin;database=BancDelTemps";
        public TestBase()
        {
            DbContextOptionsBuilder<Context> optionsBilder = new DbContextOptionsBuilder<Context>();
            optionsBilder.UseMySql(DEFAULT, new MariaDbServerVersion(new Version(10, 5, 10)));
            Context = new Context(optionsBilder.Options);
            for (int i = 0; i < Permiso.Todos.Length; i++)
            {
                if(!Context.Permisos.Any(p=>p.Nombre.Equals(Permiso.Todos[i])))
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
            User userNotValidated = Context.Users.Where(u => !u.ValidatorId.HasValue).FirstOrDefault();
            if (Equals(userNotValidated, default))
            {
                userNotValidated = new User()
                {
                    Name = "Invalidated",
                    Surname = "apellido",
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
            User userValidated = Context.GetUsersPermisos().Where(u => u.ValidatorId.HasValue && u.Permisos.Where(p=> !p.RevokedById.HasValue || p.GrantedDate > p.RevokedDate.Value).Count() == 0).FirstOrDefault();
            User userValidator = Context.GetUsersPermisos().Where(u => u.ValidatorId.HasValue && u.Permisos.Any(p => (!p.RevokedById.HasValue || p.GrantedDate > p.RevokedDate.Value) && p.Permiso.Nombre.Equals(Permiso.MODVALIDATION))).FirstOrDefault();
            if (Equals(userValidated, default))
            {
                if (Equals(userValidator, default))
                {
                    userValidator = GetUserWithPermiso(Permiso.MODVALIDATION);
                }
                userValidated = new User()
                {
                    Name = "Validated",
                    Surname="apellido",
                    Email = "Validated@emai",
                    JoinDate = DateTime.UtcNow
                };
                Context.Users.Add(userValidated);
                Context.SaveChanges();
                userValidated.Validator = userValidator;
                Context.Users.Update(userValidated);
                Context.SaveChanges();
            }
            return userValidated;
        }
        public User GetUserWithPermiso(string permiso)
        {
            User superUser = Context.GetUsersPermisos().Where(u => u.Permisos.Any(p=>(!p.RevokedById.HasValue || p.GrantedDate > p.RevokedDate.Value) && p.Permiso.Nombre.Equals(permiso))).FirstOrDefault();
            if (Equals(superUser, default))
            {

                superUser = new User()
                {
                    Name = $"User{permiso}",
                    Surname="sin apellido",
                    Email = $"User{permiso.Replace(" ","")}@email",
                    JoinDate = DateTime.UtcNow
                };
               
                Context.Users.Add(superUser);
                Context.SaveChanges();
                superUser.Validator = superUser;
                Context.Users.Update(superUser);
                Context.PermisosUsuarios.Add(new UserPermiso() { PermisoId = Context.Permisos.Where(p=>p.Nombre.Equals(permiso)).First().Id, GrantedById = superUser.Id, GrantedDate = DateTime.UtcNow, UserId = superUser.Id });
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