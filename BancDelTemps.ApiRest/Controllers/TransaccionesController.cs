﻿using BancDelTemps.ApiRest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BancDelTemps.ApiRest.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class TransaccionesController : Controller
    {
        delegate StatusCodeResult ModifyTransaccionDelegate(Transaccion transaccion, TransaccionDTO transaccionDTO);
        delegate StatusCodeResult ModifyTransaccionDelegadaDelegate(TransaccionDelegada transaccion, TransaccionDelegadaDTO transaccionDTO);
        Context Context { get; set; }
        public TransaccionesController(Context context) => Context = context;

        [HttpGet("All")]
        public IActionResult GetAll()
        {
            IActionResult result;
            User user;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                user = Context.GetUserPermisoWithTransacciones(Models.User.GetEmailFromHttpContext(HttpContext));
                result = Ok(new TransaccionesDTO(user));
            }
            else result = Forbid();
            return result;
        }

        [HttpGet("Delegadas")]
        public IActionResult GetAllDelegadas()
        {
            IActionResult result;
            User user;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                user = Context.GetUserPermisoWithTransacciones(Models.User.GetEmailFromHttpContext(HttpContext));
                result = Ok(user.TransaccionesSigned.Select(t=>new TransaccionDelegadaDTO(t)));
            }
            else
            {
                result = Forbid();
            }
            return result;
        }

        [HttpGet("{userId:int}")]
        public IActionResult GetAllUser(int userId)
        {
            IActionResult result;
            User admin;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                admin = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(HttpContext));
                if (admin.IsAdmin)
                {
                    result = Ok(new TransaccionesDTO(Context.GetUserPermisoWithTransacciones(userId)));
                }
                else result = Unauthorized();
            }
            else result = Forbid();
            return result;
        }

        [HttpGet("Delegadas/{userId:int}")]
        public IActionResult GetAllDelegadas(int userId)
        {
            IActionResult result;
            User admin;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                admin = Context.GetUserPermisoWithTransacciones(Models.User.GetEmailFromHttpContext(HttpContext));
                if (admin.IsAdmin)
                {
                    result = Ok(Context.GetUserPermisoWithTransacciones(userId).TransaccionesSigned);
                }
                else result = Unauthorized();

            }
            else
            {
                result = Forbid();
            }
            return result;
        }

        [HttpPost("")]
        public async Task<IActionResult> AddTransaccion(TransaccionDTO transaccion)
        {
            IActionResult result;
            User user;
            TransaccionDelegada transaccionDelegada;
            Operacion operacion;
            if (Equals(transaccion, default))
            {
                result = BadRequest();
            }
            else if (HttpContext.User.Identity.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(HttpContext));
                if (user.IsValidated)
                {
                    operacion = await Context.Operaciones.FindAsync(transaccion.IdOperacion);
                    if (Equals(operacion, default))
                    {
                        result = NotFound();

                    }
                    else if (operacion.Completada)
                    {
                        result = Ok();
                    }
                    else
                    {
                        if (user.Id == transaccion.IdFrom || user.IsModTransaccion)
                        {
                            operacion.Completada = true;
                            Context.Operaciones.Update(operacion);
                            await Context.Transacciones.AddAsync(transaccion.ToTransaccion());
                            await Context.SaveChangesAsync();
                            result = Ok();
                        }
                        else if (transaccion.IdTransaccionDelegada.HasValue)
                        {
                            transaccionDelegada = await Context.TransaccionesDelegadas.FindAsync(transaccion.IdTransaccionDelegada.Value);
                            if (user.Id == transaccionDelegada.UserId && transaccionDelegada.IsActiva)
                            {
                                operacion.Completada = true;
                                Context.Operaciones.Update(operacion);
                                await Context.Transacciones.AddAsync(transaccion.ToTransaccion());
                                await Context.SaveChangesAsync();
                                result = Ok();
                            }
                            else
                            {
                                result = Unauthorized();
                            }

                        }
                        else result = Unauthorized();
                    }
                }
                else
                {
                    result = Forbid();//no se puede trabajar con alguien no validado!
                }
            }
            else result = Forbid();
            return result;
        }

        [HttpPut("")]
        public async Task<IActionResult> UpdateTransaccion(TransaccionDTO transaccionDTO)
        {
            return await ModifyTransaccion(transaccionDTO, (transaccion, tDTO) =>
            {
                StatusCodeResult result;
                User user = Context.Users.Find(tDTO.IdTo);
                if (Equals(user, default))
                {
                    result = NotFound();
                }
                else if (!user.IsValidated)
                {
                    result = StatusCode((int)HttpStatusCode.Forbidden);
                }
                else
                {
                    transaccion.Minutos = tDTO.Minutos;
                    transaccion.UserToId = tDTO.IdTo;
                    transaccion.Fecha = tDTO.Fecha;
                    transaccion.UserValidator = default;
                    transaccion.UserValidatorId = default;
                    Context.Transacciones.Update(transaccion);
                    result = Ok();
                }
                return result;
            });
        }

        [HttpDelete("")]
        public async Task<IActionResult> DeleteTransaccion(TransaccionDTO transaccionDTO)
        {
            return await ModifyTransaccion(transaccionDTO, (transaccion, tDTO) =>
            {
                Context.Transacciones.Remove(transaccion);
                return Ok();
            });

        }

        async Task<IActionResult> ModifyTransaccion(TransaccionDTO transaccionDTO, ModifyTransaccionDelegate metodoModifyTransaccion)
        {
            IActionResult result;
            User user;
            Operacion operacion;
            Transaccion transaccion;
            if (Equals(transaccionDTO, default))
            {
                result = BadRequest();
            }
            else if (HttpContext.User.Identity.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(HttpContext));
                operacion = await Context.Operaciones.FindAsync(transaccionDTO.IdOperacion);
                if (Equals(operacion, default))
                {
                    result = NotFound();
                }
                else if (operacion.Completada)
                {
                    transaccion = Context.GetTransaccion(operacion);
                    if (Equals(transaccion, default))
                    {
                        result = NotFound();//el Guid de la operacion no se corresponde al de una transaccion
                    }
                    else
                    {
                        if (user.Id == transaccion.UserFromId||(transaccion.TransaccionDelegadaId.HasValue &&transaccion.TransaccionDelegada.IsActiva && transaccion.TransaccionDelegada.User.Id==user.Id) || user.IsModTransaccion)
                        {
                            result = metodoModifyTransaccion(transaccion, transaccionDTO);
                            if(result is OkResult)
                               await Context.SaveChangesAsync();
                            
                        }
                        else
                        {
                            result = Unauthorized();
                        }
                    }
                }
                else
                {
                    //no ha sido completada por lo que no existe la transaccion y no se puede actualizar
                    result = BadRequest();
                }
            }
            else result = Forbid();


            return result;
        }

       
        [HttpPost("Delegar")]
        public async Task<IActionResult> AddTransaccionDelegada(TransaccionDelegadaDTO transaccionDelegadaDTO)
        {
            IActionResult result;
            User userFrom;
            Operacion operacion;
            User userDelegado;
            TransaccionDelegada transaccionDelegada;
            if (Equals(transaccionDelegadaDTO, default))
            {
                result = BadRequest();
            }
            else if (HttpContext.User.Identity.IsAuthenticated)
            {
                userFrom = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(HttpContext));
                operacion = await Context.Operaciones.FindAsync(transaccionDelegadaDTO.IdOperacion);
                if (Equals(operacion, default))
                {
                    result = NotFound();
                } else if (operacion.Completada) {
                    result = Forbid();
                }
                else if (userFrom.Id == operacion.UserId || userFrom.IsModTransaccion)
                {
                    userDelegado = Context.GetUserPermiso(transaccionDelegadaDTO.IdUsuarioADelegar);
                    if (!Equals(userDelegado,default))
                    {
                        if (userDelegado.IsValidated)
                        {
                            if (Equals(Context.TransaccionesDelegadas.Where(t => t.OperacionId.Equals(transaccionDelegadaDTO.IdOperacion)), default))
                            {
                                transaccionDelegada = new TransaccionDelegada();
                                transaccionDelegada.UserId = transaccionDelegadaDTO.IdUsuarioADelegar;
                                transaccionDelegada.OperacionId = transaccionDelegadaDTO.IdOperacion;
                                transaccionDelegada.Inicio = transaccionDelegadaDTO.FechaInicio;
                                transaccionDelegada.Fin = transaccionDelegadaDTO.FechaFin;
                                await Context.TransaccionesDelegadas.AddAsync(transaccionDelegada);
                                await Context.SaveChangesAsync();
                                result = Ok();
                            }
                            else
                            {
                                //la operacion ya se ha delegado
                                result = BadRequest();
                            }
                        }
                        else result = Forbid();
                    }
                    else result = NotFound();

                }
                else
                {
                    result = Unauthorized();
                }
            }
            else result = Forbid();

            return result;
        }

        [HttpPut("Delegar")]
        public async Task<IActionResult> UpdateTransaccionDelegada(TransaccionDelegadaDTO transaccionDelegadaDTO)
        {
            return await ModifyTransaccionDelegada(transaccionDelegadaDTO, (transaccionDelegada, tDTO) =>
            {
                StatusCodeResult result;
                User userDelegado = Context.Users.Find(tDTO.IdUsuarioADelegar);
                if (Equals(userDelegado, default))
                {
                    result = NotFound();
                }
                else if (!userDelegado.IsValidated)
                {
                    result = StatusCode((int)HttpStatusCode.Forbidden);
                }
                else
                {
                    transaccionDelegada.Inicio = tDTO.FechaInicio;
                    transaccionDelegada.Fin = tDTO.FechaFin;
                    transaccionDelegada.UserId = tDTO.IdUsuarioADelegar;
                    Context.TransaccionesDelegadas.Update(transaccionDelegada);
                    result = Ok();
                }
                return result;
            });
        }

        [HttpDelete("Delegar")]
        public async Task<IActionResult> DeleteTransaccionDelegada(TransaccionDelegadaDTO transaccionDelegadaDTO)
        {
            return await ModifyTransaccionDelegada(transaccionDelegadaDTO, (transaccionDelegada, tDTO) =>
            {
                Context.TransaccionesDelegadas.Remove(transaccionDelegada);
                return Ok();
            });
        }

        async Task<IActionResult> ModifyTransaccionDelegada(TransaccionDelegadaDTO transaccionDelegadaDTO,ModifyTransaccionDelegadaDelegate metodoModifyTransaccion)
        {
            IActionResult result;
            User user;
            
            Operacion operacion;
            TransaccionDelegada transaccionDelegada;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(HttpContext));
                transaccionDelegada = Context.TransaccionesDelegadas.Where(t => t.OperacionId.Equals(transaccionDelegadaDTO.IdOperacion)).FirstOrDefault();

                if (Equals(transaccionDelegada, default))
                {
                    result = NotFound();
                }
                else
                {
                    operacion = await Context.Operaciones.FindAsync(transaccionDelegadaDTO.IdOperacion);
                    if (Equals(operacion, default))
                    {
                        result = NotFound();
                    }
                    else if (user.Id == operacion.UserId || user.IsModTransaccion)
                    {
                        if (transaccionDelegada.OperacionId.Equals(operacion.Id))
                        {
                            result = metodoModifyTransaccion(transaccionDelegada, transaccionDelegadaDTO);
                            if(result is OkResult)
                              await Context.SaveChangesAsync();
                       
                        }
                        else
                        {
                            result = BadRequest();
                        }
                    }
                    else result = Forbid();
                }

            }
            else result = Forbid();
            return result;
        }
    }
}
