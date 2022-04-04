﻿using BancDelTemps.ApiRest.Models;
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
        public IHttpContext ContextoHttp { get; set; }
        public AccountController(Context context, IConfiguration configuration)
        {
            Context = context;
            Configuration = configuration;
            ContextoHttp = new ContextoHttp(HttpContext);
        }

        [HttpGet("")]
        [Authorize]
        public IActionResult GetUser()
        {
            IActionResult result;
            User user;
            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetFullUser(Models.User.GetEmailFromHttpContext(ContextoHttp));
                result = Ok(new UserDTO(user));
            }
            else result = Forbid();
            return result;
        }
        [HttpGet("All")]
        [Authorize]
        public IActionResult GetAllUsers()
        {
            return GetAllUsers(0);
        }
        [HttpGet("All/ticksUTCLastUser:long")]
        [Authorize]
        public IActionResult GetAllUsers(long ticksUTCLastUser)
        {//así no hay que dar todos los usuarios siempre
            IActionResult result;
            User user;
            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (user.CanListUser)
                {
                    result = Ok(Context.GetUsersPermisosWithTransacciones().Where(u=>u.LastUpdate.Ticks>ticksUTCLastUser).Select(u => new UserDTO(u)));
                }
                else if (user.IsValidated)
                {
                    result = Ok(Context.Users.Where(u => u.IsValidated && u.LastUpdate.Ticks > ticksUTCLastUser).OrderBy(u => u.LastUpdate).Select(u => new UserBasicDTO(u)));
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
            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (user.IsAdmin)
                {
                    result = Ok((await Context.Permisos.ToListAsync()).Select(p => p.Nombre));
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
            AuthenticateResult googleResult;
            IActionResult result;
            User user;
            

            googleResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (googleResult.Succeeded)
            {
                user = new User(googleResult.Principal);
                result =await GetTokenUser(user);
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
            return Ok(token.WriteToken());
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

            if (ContextoHttp.IsAuthenticated)
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
                                permiso = Context.Permisos.Where(p => p.Nombre.Equals(permisoLow)).FirstOrDefault();
                                if (!Equals(permiso, default))
                                {
                                    try
                                    {
                                        userPermiso = Context.PermisosUsuarios.Where(p => p.PermisoId.Equals(permiso.Id) && p.UserId.Equals(userToAdd.Id)).FirstOrDefault();
                                        if (!Equals(userPermiso, default))
                                        {
                                            if(! userPermiso.IsActive){//asi si no es necesario no pierde quien y cuando se dio por última vez. 
                                            userPermiso.GrantedBy = userGranter;
                                            userPermiso.GrantedDate = DateTime.UtcNow;
                                            Context.PermisosUsuarios.Update(userPermiso);
                                           } 
                                        }
                                        else
                                        {
                                            Context.PermisosUsuarios.Add(new UserPermiso(userGranter, userToAdd, permiso));
                                        }
                                        Context.SaveChanges();
                                        permisosOk.Add(permisoUserDTO.Permisos[i]);
                                    }
                                    catch { }
                                    
                                }

                            }
                            result = Ok(permisosOk);
                        }
                        else result = Forbid();//el usuario aun no se ha validado!
                        
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

            if (ContextoHttp.IsAuthenticated)
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
                                permiso = Context.Permisos.Where(p => p.Nombre.Equals(permisoLow)).FirstOrDefault();
                                if (!Equals(permiso, default))
                                {
                                    userPermiso = Context.PermisosUsuarios.Where(p => p.PermisoId.Equals(permiso.Id) && p.UserId.Equals(userToRemove.Id)).FirstOrDefault();
                                    if (!Equals(userPermiso, default))
                                    {
                                        try
                                        {
                                            if(userPermiso.IsActive){
                                                userPermiso.RevokedBy = userRevoker;
                                                userPermiso.RevokedDate = DateTime.UtcNow;
                                                Context.PermisosUsuarios.Update(userPermiso);
                                                Context.SaveChanges();
                                            }
                                            permisosOk.Add(permisoUserDTO.Permisos[i]);
                                        }
                                        catch { }
                                        

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
            else result = Forbid();
            return result;
        }
        [HttpPut("Validate/{userId:long}")]
        [Authorize]
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
                result = Forbid();
            }
            return result;
        }


        //gestionar todo lo que se puede configurar de un usuario
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateUser(UserDTO userToUpdateData)
        {
            IActionResult result;
            User user,userToUpdate;
            if (ContextoHttp.IsAuthenticated)
            {
                user= Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (Equals(userToUpdateData.Email, user.Email))
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
                    userToUpdate = Context.GetFullUser(userToUpdateData.Email);
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
            else result = Forbid();
            return result;
        }

        private async Task<IActionResult> UpdateDataUser(User userToUpdate, UserDTO userToUpdateData)
        {
            IActionResult result=Ok();
            if (!Equals(userToUpdateData.NewEmail, default) && !Equals(userToUpdateData.NewEmail, userToUpdateData.Email))
            {
                if (Equals(Context.GetUserPermiso(userToUpdateData.NewEmail), default))
                {
                    userToUpdate.Email = userToUpdateData.NewEmail;
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
    }
}
