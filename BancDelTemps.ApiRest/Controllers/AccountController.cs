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

        [HttpGet("")]
        [Authorize]
        public IActionResult GetUser()
        {
            IActionResult result;
            User user;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                user = Context.GetFullUser(Models.User.GetEmailFromHttpContext(HttpContext));
                result = Ok(new UserDTO(user));
            }
            else result = Forbid();
            return result;
        }

        [HttpGet("All")]
        [Authorize]
        public IActionResult GetAllUsers()
        {
            IActionResult result;
            User user;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(HttpContext));
                if (user.CanListUser)
                {
                    result = Ok(Context.GetUsersPermisosWithTransacciones().Select(u => new UserDTO(u)));
                }
                else if(user.IsValidated) {
                    result = Ok(Context.Users.Where(u=>u.IsValidated).Select(u => new UserBasicDTO(u)));
                }
                else
                {
                    result = Unauthorized();
                }
            }
            else result = Forbid();
            return result;
        }


        [HttpGet("Permisos/All")]
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
            else result = Forbid();
            return result;
        }
       
        [HttpGet("Login")]
        public IActionResult GoogleLogin()
        {
            AuthenticationProperties properties;
            properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(GetToken)) };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("Token")]
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

        [HttpPut("Permisos")]
        [Authorize]
        public IActionResult PermissionsPut(PermisoUserDTO permisoUserDTO)
        {
            IActionResult result;
            User userGranter;
            User userToAdd;
            Permiso permiso;
            UserPermiso userPermiso;
            List<string> permisosOk;
            string permisoLow;

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                userGranter = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(HttpContext));
                
                if (userGranter.IsAdmin)
                {
                    userToAdd = Context.GetUserPermiso(permisoUserDTO.EmailUser);
                    if (!Equals(userToAdd, default))
                    {
                        if (userGranter.Id.Equals(userToAdd.Id))
                        {
                            result = Unauthorized();//no se puede dar permisos a si mismo
                        }
                        else if(userToAdd.IsValidated)
                        {
                            permisosOk = new List<string>();
                            for (int i = 0; i < permisoUserDTO.Permisos.Length; i++)
                            {
                                permisoLow = permisoUserDTO.Permisos[i].ToLower();
                                permiso = Context.Permisos.Where(p => p.Nombre.Equals(permisoLow)).FirstOrDefault();
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
                                    permisosOk.Add(permisoUserDTO.Permisos[i]);
                                }
                              
                            }
                            result = Ok(permisosOk);
                        }
                        else
                        {
                            result = Forbid();//el usuario aun no se ha validado!
                        }
                    }
                    else result = NotFound();
              
                }
                else result = Unauthorized();
            }
            else result = Forbid();
            return result;
        }
      
        [HttpDelete("Permisos")]
        [Authorize]
        public IActionResult PermissionsDelete(PermisoUserDTO permisoUserDTO)
        {
            IActionResult result;
            User userRevoker;
            User userToRemove;
            Permiso permiso;
            UserPermiso userPermiso;
            List<string> permisosOk;
            string permisoLow;

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                userRevoker = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(HttpContext));

                if (userRevoker.IsAdmin)
                {
                    userToRemove = Context.GetUserPermiso(permisoUserDTO.EmailUser);
                    if (!Equals(userToRemove, default))
                    {
                        if (userRevoker.Id.Equals(userToRemove.Id))
                        {
                            result = Unauthorized();//no se puede quitar permisos a si mismo
                        }
                        else
                        {
                            permisosOk = new List<string>();
                            for (int i = 0; i < permisoUserDTO.Permisos.Length; i++)
                            {
                                permisoLow = permisoUserDTO.Permisos[i].ToLower();
                                permiso = Context.Permisos.Where(p => p.Nombre.Equals(permisoLow)).FirstOrDefault();
                                if (!Equals(permiso, default))
                                {
                                    userPermiso = Context.PermisosUsuarios.Where(p => p.PermisoId.Equals(permiso.Id) && p.UserId.Equals(userToRemove.Id)).FirstOrDefault();
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
                                        permisosOk.Add(permisoUserDTO.Permisos[i]);

                                    }
                           

                                }
                              
                            }
                            result = Ok(permisosOk);
                        }
                    }
                    else result = NotFound();

                }
                else result = Unauthorized();
            }
            else result = Forbid();
            return result;
        }

        [HttpGet("All/ToValidate")]
        [Authorize]
        public IActionResult GetAllInValidatedUsers()
        {
            IActionResult result;
            User user;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(HttpContext));
                if (user.IsModValidate)
                {
                    result = Ok(Context.Users.Where(u => !u.IsValidated).OrderBy(u=>u.JoinDate).Select(u => new UserBasicDTO(u)));
                }
                else
                {
                    result = Unauthorized();
                }
            }
            else result = Forbid();
            return result;
        }
        [HttpPut("Validate/{userId:long}")]
        [Authorize]
        public async Task<IActionResult> ValidarUsuario(long userId)
        {
            IActionResult result;
            User modValidation, user;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                modValidation = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(HttpContext));
                if (modValidation.IsModValidate)
                {
                    user = await Context.Users.FindAsync(userId);
                    if (Equals(user, default))
                    {
                        result = NotFound();
                    }
                    else if(!user.IsValidated)
                    {
                        
                        user.Validator = modValidation;
                        Context.Users.Update(user);
                        await Context.SaveChangesAsync();
                        result = Ok();
                    }
                    else
                    {
                        result = Ok();//ya está validado!!
                    }
                }
                else result = Unauthorized();
            }
            else
            {
                result = Forbid();
            }
            return result;
        }

    }
}
