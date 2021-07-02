using BancDelTemps.ApiRest.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace BancDelTemps.ApiRest.Controllers
{
    [AllowAnonymous]
    [Route("[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        Context Context { get; set; }
        IConfiguration Configuration { get; set; }
        public AccountController(Context context,IConfiguration configuration)
        {
            Context = context;
            Configuration = configuration;
        }

        [HttpGet]
        [Route("")]
        [Authorize]
        public IActionResult GetUser()
        {
            IActionResult result;
            User user;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                user = Context.GetUserPermisoWithTransacciones(Models.User.GetEmailFromHttpContext(HttpContext));
                result = Ok(new UserDTO(user));
            }
            else result = Unauthorized();
            return result;
        }

        [HttpGet]
        [Route("all")]
        [Authorize]
        public IActionResult GetAllUsers()
        {
            IActionResult result;
            User user;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(HttpContext));
                if (user.IsAdmin)
                {
                    result = Ok(Context.GetUsersPermisosWithTransacciones().Select(u=>new UserDTO(u)));
                }
                else result = Unauthorized();
            }
            else result = Unauthorized();
            return result;
        }
        [HttpGet]
        [Route("allPermisos")]
        [Authorize]
        public async Task<IActionResult> GetAllPermisos()
        {
            IActionResult result;
            User user;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(HttpContext));
                if (user.IsAdmin)
                {
                    result = Ok((await Context.Permisos.ToListAsync()).Select(p=>p.Nombre));
                }
                else result = Unauthorized();
            }
            else result = Unauthorized();
            return result;
        }
        [HttpGet]
        [Route("login")]
        public IActionResult GoogleLogin()
        {
            AuthenticationProperties properties;
            properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(GetToken)) };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        [Route("token")]
        public async Task<IActionResult> GetToken()
        {
            IActionResult result;
            User user;
            JwtSecurityToken token;
            AuthenticateResult googleResult;

            googleResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (googleResult.Succeeded)
            {
                user = new User(googleResult.Principal);
                if (!Context.ExistUser(user))
                {
                    //no existe pues lo añado
                    try
                    {
                        Context.Users.Add(user);
                        await Context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        throw;
                    }
                }

                token = Context.GetUserPermiso(user).GetToken(Configuration);//así obtengo la información guardada
                result = Ok(token.WriteToken());
            }
            else
            {
                result = Unauthorized();
            }
            return result;


        }

        [HttpPut]
        [Route("permisos")]
        [Authorize]
        public IActionResult PermissionsPut(PermisoUserDTO permisoUserDTO)
        {
            IActionResult result;
            User userGranter;
            User userToAdd;
            Permiso permiso;
            UserPermiso userPermiso;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                userGranter = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(HttpContext));
                
                if (userGranter.IsAdmin)
                {
                    userToAdd = Context.GetUserPermiso(permisoUserDTO.EmailUser);
                    if (!Equals(userToAdd, default))
                    {
                        permiso = Context.Permisos.Where(p => p.Nombre.Equals(permisoUserDTO.Permiso)).FirstOrDefault();
                        if (!Equals(permiso, default))
                        {
                            try
                            {
                                userPermiso = Context.PermisosUsuarios.Where(p => p.PermisoId.Equals(permiso.Id) && p.UserId.Equals(userToAdd.Id)).FirstOrDefault();
                                if (!Equals(userPermiso, default))
                                {
                                    userPermiso.GrantedBy = userGranter;
                                    userPermiso.GrantedDate = DateTime.UtcNow;
                                    Context.PermisosUsuarios.Update(userPermiso);
                                }
                                else
                                {
                                    Context.PermisosUsuarios.Add(new UserPermiso(userGranter, userToAdd, permiso));
                                }
                                Context.SaveChanges();
                               
                            }
                            catch { }
                            result = Ok();
                        }
                        else result = NotFound();
                    }
                    else result = NotFound();
              
                }
                else result = Unauthorized();
            }
            else result = Unauthorized();
            return result;
        }
        [HttpDelete]
        [Route("permisos")]
        [Authorize]
        public IActionResult PermissionsDelete(PermisoUserDTO permisoUserDTO)
        {
            IActionResult result;
            User userRevoker;
            User userToRemove;
            Permiso permiso;
            UserPermiso userPermiso;

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                userRevoker = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(HttpContext));

                if (userRevoker.IsAdmin)
                {
                    userToRemove = Context.GetUserPermiso(permisoUserDTO.EmailUser);
                    if (!Equals(userToRemove, default))
                    {
                        permiso = Context.Permisos.Where(p => p.Nombre.Equals(permisoUserDTO.Permiso)).FirstOrDefault();
                        if (!Equals(permiso, default))
                        {
                            userPermiso =  Context.PermisosUsuarios.Where(p => p.PermisoId.Equals(permiso.Id) && p.UserId.Equals(userToRemove.Id)).FirstOrDefault();
                            if (!Equals(userPermiso, default))
                            {
                                try
                                {
                                    userPermiso.RevokedBy = userRevoker;
                                    userPermiso.RevokedDate = DateTime.UtcNow;
                                    Context.PermisosUsuarios.Update(userPermiso);
                                    Context.SaveChanges();
                                }
                                catch { }
                                result = Ok();

                            }
                            else result = NotFound();

                        }
                        else result = NotFound();
                    }
                    else result = NotFound();

                }
                else result = Unauthorized();
            }
            else result = Unauthorized();
            return result;
        }

    }
}
