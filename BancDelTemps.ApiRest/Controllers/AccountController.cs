using BancDelTemps.ApiRest.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace BancDelTemps.ApiRest.Controllers
{
    /// <summary>
    /// Este controlador sirve para tratar todo lo relativo a las cuentas de usuario
    /// </summary>
    [AllowAnonymous]
    [Route("[controller]")]
    [ApiController]
    [Produces("application/json", new string[] { "text/plain", "text/json" })]
    public class AccountController : Controller
    {
        /// <summary>
        /// Son los servidores de autenticación de la aplicación
        /// </summary>
        public static string[] AccountEmailServerValid => new string[] {
                                                                        "gmail.com",
                                                                        "googlemail.com"
                                                                      };
        Context Context { get; set; }
        IConfiguration Configuration { get; set; }
        /// <summary>
        /// Es para los Test
        /// </summary>
        public IHttpContext ContextoHttp { get; set; }
        public AccountController(Context context, IConfiguration configuration)
        {
            Context = context;
            Configuration = configuration;
            ContextoHttp = new ContextoHttp(HttpContext);
        }
        /// <summary>
        /// Obtiene toda la información del usuario logueado
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDTO))]
     
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn,typeof(ContentResult))]

        public IActionResult GetUser()
        {
            IActionResult result;
            User user;
            if (ContextoHttp.IsAuthenticated)
            {

                user = Context.GetFullUser(Models.User.GetEmailFromHttpContext(ContextoHttp));
                result = Ok(new UserDTO(user));

            }
            else result = this.NotLoggedIn();
            return result;
        }

        [HttpGet("All")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDTO))]
        [ProducesResponseType(StatusCodes.Status206PartialContent, Type = typeof(UserBasicDTO))]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public IActionResult GetAllUsers()
        {
            return GetAllUsers(0);
        }
        [HttpGet("All/ticksUTCLastTime:long")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDTO))]
        [ProducesResponseType(StatusCodes.Status206PartialContent, Type = typeof(UserBasicDTO))]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public IActionResult GetAllUsers(long ticksUTCLastTime)
        {//así no hay que dar todos los usuarios siempre
            IActionResult result;
            User user;
            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (user.CanListUser)
                {
                    result = Ok(Context.GetUsersPermisosWithTransacciones()
                                        .Where(u => u.ValidatedRegister && u.LastUpdate.Ticks > ticksUTCLastTime)
                                        .OrderBy(u => u.LastUpdate)
                                        .Select(u => new UserDTO(u))
                                );
                }
                else if (user.IsValidated)
                {
                    result = StatusCode(StatusCodes.Status206PartialContent,Context.Users.Where(u => u.IsValidated && u.LastUpdate.Ticks > ticksUTCLastTime)
                                             .OrderBy(u => u.LastUpdate)
                                             .Select(u => new UserBasicDTO(u))
                               );
                }
                else
                {//así pueden ponerse en contacto con los responsables de la validación
                    result = StatusCode(StatusCodes.Status206PartialContent, Context.Users.Include(u => u.Permisos)
                                             .Where(u => u.PermisosActivosName.Any(p => Equals(p, Permiso.MODVALIDATION) || Equals(p, Permiso.ADMIN)))
                                             .OrderBy(u => u.LastUpdate)
                                             .Select(u => new UserBasicDTO(u))
                               );
                }
            }
            else result = this.NotLoggedIn();
            return result;
        }

        //gestionar todo lo que se puede configurar de un usuario
        [HttpPut]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> UpdateUser(UserDTO userToUpdateData)
        {
            IActionResult result;
            User user, userToUpdate;
            if (ContextoHttp.IsAuthenticated)
            {
                if (!Equals(userToUpdateData, default))
                {
                    user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                    if (string.Equals(userToUpdateData.Email, user.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        //puede cambiar algunos datos
                        user.StartHolidays = userToUpdateData.StartHolidays;
                        user.EndHolidays = userToUpdateData.EndHolidays;

                        user.LastUpdateDate = DateTime.Now;
                        Context.Users.Update(user);
                        await Context.SaveChangesAsync();
                        result = Ok();
                    }
                    else if (user.IsModUser)
                    {
                        userToUpdate = Context.GetUserPermiso(userToUpdateData.Email);
                        if (Equals(userToUpdate, default))
                        {
                            result = NotFound();
                        }
                        else if (userToUpdate.PermisosActivos.Any())
                        {
                            //only admin can edit
                            if (user.IsAdmin)
                            {
                                //puede cambiarlos todos

                                result = await UpdateDataUser(userToUpdate, userToUpdateData);
                            }
                            else
                            {
                                result = Unauthorized();
                            }
                        }
                        else
                        {


                            //puede cambiarlos todos
                            result = await UpdateDataUser(userToUpdate, userToUpdateData);
                        }

                    }
                    else
                    {
                        result = Unauthorized();
                    }
                }
                else result = BadRequest();

            }
            else result = this.NotLoggedIn();
            return result;
        }

        private async Task<IActionResult> UpdateDataUser(User userToUpdate, UserDTO userToUpdateData)
        {
            IActionResult result = Ok();
            if (!Equals(userToUpdateData.NewEmail, default) && !Equals(userToUpdateData.NewEmail, userToUpdateData.Email))
            {
                if (Equals(Context.GetUserPermiso(userToUpdateData.NewEmail), default))
                {
                    if (ValidateEmail(userToUpdateData.NewEmail))
                    {
                        userToUpdate.Email = userToUpdateData.NewEmail.ToLower();
                    }
                    else result = BadRequest($"El email '{userToUpdateData.NewEmail}' no está bien formado o no es de un servidor valido!");
                }
                else
                {
                    result = Conflict($"El email '{userToUpdateData.NewEmail}' ya está en uso");
                }
            }
            if (result is OkResult)
            {
                if (!Equals(userToUpdateData.Name, default))
                {
                    userToUpdate.Name = userToUpdateData.Name;
                }
                if (!Equals(userToUpdateData.Surname, default))
                {
                    userToUpdate.Surname = userToUpdateData.Surname;
                }
                userToUpdate.StartHolidays = userToUpdateData.StartHolidays;
                userToUpdate.EndHolidays = userToUpdateData.EndHolidays;
                userToUpdate.JoinDate = userToUpdateData.JoinDate;

                userToUpdate.LastUpdateDate = DateTime.Now;
                Context.Users.Update(userToUpdate);
                await Context.SaveChangesAsync();
            }

            return result;
        }

        public static bool ValidateEmail(string emailToValidate)
        {
            //se asegura que el email esté bien formado 
            const string VALIDATEEMAILPATTERN = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

            string server;
            bool isValid = !string.IsNullOrEmpty(emailToValidate) && System.Text.RegularExpressions.Regex.IsMatch(emailToValidate, VALIDATEEMAILPATTERN);
            if (isValid)
            {
                server = emailToValidate.Split('@')[1];
                isValid = AccountEmailServerValid.Any(s => string.Equals(s, server, StringComparison.OrdinalIgnoreCase));
            }
            return isValid;
        }

        [HttpGet("Permisos/All")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PermisoDTO[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> GetAllPermisos()
        {
            IActionResult result;
            User user;
            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (user.IsAdmin)
                {
                    result = Ok((await Context.Permisos.ToListAsync()).Select(p =>new PermisoDTO(p)));
                }
                else result = Unauthorized();
            }
            else result = this.NotLoggedIn();
            return result;
        }

        [HttpGet("Login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GoogleLogin()
        {
            AuthenticationProperties properties;
            properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(GetToken)) };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("Token")]
        [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(LoginDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> GetToken()
        {
            AuthenticateResult googleResult;
            IActionResult result;
            User user;


            googleResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (googleResult.Succeeded)
            {
                user = new User(googleResult.Principal);
                result = await GetTokenUser(user);
            }
            else
            {
                result = Unauthorized();
            }
            return result;


        }
        //así cuando haga los test unitarios podré tener los token
        internal async Task<IActionResult> GetTokenUser(User user)
        {
            JwtSecurityToken token;
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
            return Ok(new LoginDTO(token));
        }

        [HttpPut("Register")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> Register()
        {
            IActionResult result;
            User user;
            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));

                if (!user.ValidatedRegister)
                {
                    user.ValidatedRegister = true;
                    Context.Users.Update(user);
                    await Context.SaveChangesAsync();
                }
                result = Ok();



            }
            else result = this.NotLoggedIn();
            return result;
        }
        [HttpDelete("Register")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> Unregister()
        {
            IActionResult result;
            User user;
            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));

                if (user.CanSelfUnregister)
                {
                    Context.Users.Remove(user);
                    await Context.SaveChangesAsync();
                    result = Ok();
                }
                else result = Unauthorized();




            }
            else result = this.NotLoggedIn();
            return result;
        }
        [HttpDelete("Register/userId:long")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(UserBasicDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> UnRegister(long userId)
        {
            IActionResult result;
            User user;
            User userToRemove;
            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (user.IsModUser)
                {
                    userToRemove = await Context.Users.Where(u => Equals(u.Id, userId)).FirstOrDefaultAsync();
                    if (!Equals(userToRemove, default))
                    {
                        Context.Users.Remove(userToRemove);
                        await Context.SaveChangesAsync();
                        result = Ok(new UserBasicDTO(userToRemove));
                    }
                    else result = NotFound();
                }
                else result = Unauthorized();
            }
            else result = this.NotLoggedIn();

            return result;
        }
        [HttpDelete("Register/AllUndone")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(UserBasicDTO[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> UnRegisterAllUnDone()
        {
            IActionResult result;
            User admin;
            User[] users;
            if (ContextoHttp.IsAuthenticated)
            {
                admin = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (admin.IsAdmin)
                {
                    users = await Context.Users.Where(u => !u.ValidatedRegister && !u.HasTimeToUnregister).ToArrayAsync();
                    Context.Users.RemoveRange(users);
                    await Context.SaveChangesAsync();
                    result = Ok(users.Select(u => new UserBasicDTO(u)));
                }
                else result = Unauthorized();
            }
            else result = this.NotLoggedIn();
            return result;
        }

        [HttpPut("Permisos")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(PermisoDTO[]))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerResponse(OwnStatusCodes.NotValidated, OwnMessage.NotValidated, typeof(ContentResult))]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> PermissionsPut(PermisoUserDTO permisoUserDTO)
        {
            IActionResult result;
            User userGranter;
            User userToAdd;
            Permiso permiso;
            UserPermiso userPermiso;
            List<string> permisosOk;
            string permisoLow;

            if (ContextoHttp.IsAuthenticated)
            {
                if (!Equals(permisoUserDTO, default))
                {
                    userGranter = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));

                    if (userGranter.IsAdmin)
                    {
                        userToAdd = Context.GetUserPermiso(permisoUserDTO.EmailUser);
                        if (!Equals(userToAdd, default))
                        {
                            if (userGranter.Id.Equals(userToAdd.Id))
                            {
                                result = Unauthorized();//no se puede dar permisos a si mismo
                            }
                            else if (userToAdd.IsValidated)
                            {
                                permisosOk = new List<string>();
                                for (int i = 0; i < permisoUserDTO.Permisos.Length; i++)
                                {
                                    permisoLow = permisoUserDTO.Permisos[i].ToLower();
                                    permiso = await Context.Permisos.Where(p => p.Nombre.Equals(permisoLow)).FirstOrDefaultAsync();
                                    if (!Equals(permiso, default))
                                    {
                                        try
                                        {
                                            userPermiso = await Context.PermisosUsuarios.Where(p => p.PermisoId.Equals(permiso.Id) && p.UserId.Equals(userToAdd.Id)).FirstOrDefaultAsync();
                                            if (!Equals(userPermiso, default))
                                            {
                                                if (!userPermiso.IsActive)
                                                {//asi si no es necesario no pierde quien y cuando se dio por última vez. 
                                                    userPermiso.GrantedBy = userGranter;
                                                    userPermiso.GrantedDate = DateTime.UtcNow;
                                                    Context.PermisosUsuarios.Update(userPermiso);
                                                }
                                            }
                                            else
                                            {
                                                Context.PermisosUsuarios.Add(new UserPermiso(userGranter, userToAdd, permiso));
                                            }
                                            await Context.SaveChangesAsync();
                                            permisosOk.Add(permisoUserDTO.Permisos[i]);
                                        }
                                        catch { }

                                    }

                                }
                                result = Ok(permisosOk.Select(p=>new PermisoDTO(p)));
                            }
                            else result = this.NotValidated();

                        }
                        else result = NotFound();

                    }
                    else result = Unauthorized();
                }
                else result = BadRequest();
            }
            else result = this.NotLoggedIn();

            return result;
        }

        [HttpDelete("Permisos")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PermisoDTO[]))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> PermissionsDelete(PermisoUserDTO permisoUserDTO)
        {
            IActionResult result;
            User userRevoker;
            User userToRemove;
            Permiso permiso;
            UserPermiso userPermiso;
            List<string> permisosOk;
            string permisoLow;

            if (ContextoHttp.IsAuthenticated)
            {
                if (!Equals(permisoUserDTO, default))
                {
                    userRevoker = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));

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
                                    permiso = await Context.Permisos.Where(p => p.Nombre.Equals(permisoLow)).FirstOrDefaultAsync();
                                    if (!Equals(permiso, default))
                                    {
                                        userPermiso = await Context.PermisosUsuarios.Where(p => p.PermisoId.Equals(permiso.Id) && p.UserId.Equals(userToRemove.Id)).FirstOrDefaultAsync();
                                        if (!Equals(userPermiso, default))
                                        {
                                            try
                                            {
                                                if (userPermiso.IsActive)
                                                {
                                                    userPermiso.RevokedBy = userRevoker;
                                                    userPermiso.RevokedDate = DateTime.UtcNow;
                                                    Context.PermisosUsuarios.Update(userPermiso);
                                                    await Context.SaveChangesAsync();
                                                }
                                                permisosOk.Add(permisoUserDTO.Permisos[i]);
                                            }
                                            catch { }


                                        }

                                        
                                    }

                                }
                                result = Ok(permisosOk.Select(p=>new PermisoDTO(p)));
                            }
                        }
                        else result = NotFound();

                    }
                    else result = Unauthorized();
                }
                else result = BadRequest();
            }
            else result = this.NotLoggedIn();
            return result;
        }

        [HttpGet("All/ToValidate")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserBasicDTO[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public IActionResult GetAllInValidatedUsers()
        {
            IActionResult result;
            User user;
            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (user.IsModValidation)
                {
                    result = Ok(Context.Users.Where(u => !u.IsValidated).OrderBy(u => u.JoinDate).Select(u => new UserBasicDTO(u)));
                }
                else
                {
                    result = Unauthorized();
                }
            }
            else result = this.NotLoggedIn();
            return result;
        }
        [HttpPut("Validate/{userId:long}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> ValidarUsuario(long userId)
        {
            IActionResult result;
            User modValidation, user;
            if (ContextoHttp.IsAuthenticated)
            {
                modValidation = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (modValidation.IsModValidation)
                {
                    user = await Context.Users.FindAsync(userId);
                    if (Equals(user, default))
                    {
                        result = NotFound();
                    }
                    else if (!user.IsValidated)
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
                result = this.NotLoggedIn();
            }
            return result;
        }


    }
}
