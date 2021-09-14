using BancDelTemps.ApiRest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        delegate IActionResult ModifyTransaccionDelegate(Transaccion transaccion, TransaccionDTO transaccionDTO);
        delegate IActionResult ModifyTransaccionDelegadaDelegate(TransaccionDelegada transaccion, TransaccionDelegadaDTO transaccionDTO);

        Context Context { get; set; }
        public IHttpContext ContextoHttp { get; set; }
        public TransaccionesController(Context context)
        {
            Context = context;
            ContextoHttp = new ContextoHttp(HttpContext);
        }

        [HttpGet("{idTransaccion:long}")]
        public IActionResult Get(long idTransaccion)
        {

            IActionResult result;
            User user;
            Transaccion transaccion;
            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermisoWithTransacciones(Models.User.GetEmailFromHttpContext(ContextoHttp));
                transaccion = user.TransaccionesIn.Where(t => t.Id == idTransaccion).FirstOrDefault();
                if (Equals(transaccion, default))
                {
                    transaccion = user.TransaccionesFrom.Where(t => t.Id == idTransaccion).FirstOrDefault();
                }
                if (Equals(transaccion, default))
                {
                    result = NotFound();
                }
                else
                {
                    result = Ok(new TransaccionDTO(transaccion));
                }
            }
            else result = Forbid();

            return result;
        }
        [HttpGet("Delegar/{idTransaccionDelegada:long}")]
        public IActionResult GetDelegada(long idTransaccionDelegada)
        {
            //falta probar
            IActionResult result;
            User user;
            TransaccionDelegada transaccionDelegada;
            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermisoWithTransacciones(Models.User.GetEmailFromHttpContext(ContextoHttp));
                transaccionDelegada = user.TransaccionesSigned.Where(t => t.Id == idTransaccionDelegada).FirstOrDefault();
                if (Equals(transaccionDelegada, default))
                {
                    result = NotFound();
                }
                else
                {
                    result = Ok(new TransaccionDelegadaDTO(transaccionDelegada));
                }
            }
            else result = Forbid();

            return result;
        }
        [HttpGet("Validar/All/{ticksLastUpdate:long}")]
        public IActionResult GetToValidateAll(long ticksLastUpdate)
        {
            IActionResult result;
            User validador;
            if (ContextoHttp.IsAuthenticated)
            {
                validador = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (validador.IsModTransaccion)
                {
                    result = Ok(Context.Transacciones
                                       .Include(t => t.Operacion)
                                       .Where(t => !t.Operacion.IsRevisada && t.Fecha.Ticks > ticksLastUpdate)
                                       .Select(t => new TransaccionDTO(t))
                            );
                }
                else result = Unauthorized();
            }
            else result = Forbid();
            return result;

        }
        [HttpPost("Validar/{idOpracion:long}/{isValid:bool}")]
        public async Task<IActionResult> Validate(long idOpracion, bool isValid)
        {
            ActionResult result;
            User validador;
            Operacion operacion;
            if (ContextoHttp.IsAuthenticated)
            {
                validador = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (validador.IsModTransaccion)
                {
                    operacion = await Context.Operaciones.FindAsync(idOpracion);
                    if (Equals(operacion, default))
                    {
                        result = NotFound();
                    }
                    else
                    {
                        if (!operacion.IsRevisada)
                        {
                            operacion.Revisor = validador;
                            operacion.IsValid = isValid;
                            await Context.SaveChangesAsync();
                        }
                        result = Ok(operacion);
                    }
                }
                else result = Unauthorized();
            }
            else result = Forbid();
            return result;
        }
        [HttpGet("All/{ticksLastUpdate:long}")]
        public IActionResult GetAll(long ticksLastUpdate)
        {
            IActionResult result;
            User user;
            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermisoWithTransacciones(Models.User.GetEmailFromHttpContext(ContextoHttp));
                result = Ok(new TransaccionesDTO(user, Context, ticksLastUpdate));
            }
            else result = Forbid();
            return result;
        }

        [HttpGet("Delegadas")]
        public IActionResult GetAllDelegadas()
        {
            IActionResult result;
            User user;
            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermisoWithTransacciones(Models.User.GetEmailFromHttpContext(ContextoHttp));
                result = Ok(user.TransaccionesSigned.Select(t => new TransaccionDelegadaDTO(t)));
            }
            else
            {
                result = Forbid();
            }
            return result;
        }

        [HttpGet("User/{userId:long}/{ticksLastUpdate:long}")]
        public IActionResult GetAllUser(long userId, long ticksLastUpdate)
        {
            IActionResult result;
            User admin;
            if (ContextoHttp.IsAuthenticated)
            {
                admin = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (admin.IsAdmin)
                {
                    result = Ok(new TransaccionesDTO(Context.GetUserPermisoWithTransacciones(userId), Context, ticksLastUpdate));
                }
                else result = Unauthorized();
            }
            else result = Forbid();
            return result;
        }

        [HttpGet("User/Delegadas/{userId:long}")]
        public IActionResult GetAllDelegadasUser(long userId)
        {
            IActionResult result;
            User admin;
            if (ContextoHttp.IsAuthenticated)
            {
                admin = Context.GetUserPermisoWithTransacciones(Models.User.GetEmailFromHttpContext(ContextoHttp));
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
        public async Task<IActionResult> AddTransaccion(TransaccionDTO transaccionDTO)
        {
            IActionResult result;
            User user;
            TransaccionDelegada transaccionDelegada;
            Operacion operacion;
            if (Equals(transaccionDTO, default))
            {
                result = BadRequest();
            }
            else if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (user.IsValidated)
                {
                    operacion = await Context.Operaciones.FindAsync(transaccionDTO.IdOperacion);
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
                        if (user.Id == transaccionDTO.IdFrom || user.IsModTransaccion)
                        {
                            operacion.Completada = true;
                            Context.Operaciones.Update(operacion);
                            await Context.Transacciones.AddAsync(transaccionDTO.ToTransaccion());
                            await Context.SaveChangesAsync();
                            result = Ok();
                        }
                        else
                        {
                            transaccionDelegada = Context.GetTransaccionDelegada(operacion);
                            if (!Equals(transaccionDelegada, default) && user.Id == transaccionDelegada.UserId && transaccionDelegada.IsActiva)
                            {
                                operacion.Completada = true;
                                Context.Operaciones.Update(operacion);
                                await Context.Transacciones.AddAsync(transaccionDTO.ToTransaccion());
                                await Context.SaveChangesAsync();
                                result = Ok();
                            }
                            else
                            {
                                result = Unauthorized();
                            }

                        }

                    }
                }
                else result = Forbid();//no se puede trabajar con alguien no validado!

            }
            else result = Forbid();
            return result;
        }

        [HttpPut("")]
        public async Task<IActionResult> UpdateTransaccion(TransaccionDTO transaccionDTO)
        {
            return await ModifyTransaccion(transaccionDTO, (transaccion, tDTO) =>
            {
                IActionResult result;
                User user = Context.Users.Find(tDTO.IdTo);
                if (Equals(user, default))
                {
                    result = NotFound();
                }
                else if (!user.IsValidated)
                {
                    result = Forbid();
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
            TransaccionDelegada transaccionDelegada;
            if (Equals(transaccionDTO, default))
            {
                result = BadRequest();
            }
            else if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
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
                        transaccionDelegada = Context.GetTransaccionDelegada(operacion);
                        if (user.Id == transaccion.UserFromId || (!Equals(transaccionDelegada, default) && transaccionDelegada.IsActiva && transaccionDelegada.User.Id == user.Id) || user.IsModTransaccion)
                        {
                            if (transaccion.CanBack || user.IsModTransaccion)
                            {
                                result = metodoModifyTransaccion(transaccion, transaccionDTO);
                                if (result is OkResult)
                                {
                                    await Context.SaveChangesAsync();
                                }
                            }
                            else result = Unauthorized();


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
            else if (ContextoHttp.IsAuthenticated)
            {
                userFrom = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                operacion = await Context.Operaciones.FindAsync(transaccionDelegadaDTO.IdOperacion);
                if (Equals(operacion, default))
                {
                    result = NotFound();
                }
                else if (operacion.Completada)
                {
                    result = Forbid();
                }
                else if (userFrom.Id == operacion.UserId || userFrom.IsModTransaccion)
                {
                    userDelegado = Context.GetUserPermiso(transaccionDelegadaDTO.IdUsuarioADelegar);
                    if (!Equals(userDelegado, default))
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
                                result = Forbid();
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
                IActionResult result;
                User userDelegado = Context.Users.Find(tDTO.IdUsuarioADelegar);
                if (Equals(userDelegado, default))
                {
                    result = NotFound();
                }
                else if (!userDelegado.IsValidated)
                {
                    result = Forbid();
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

        async Task<IActionResult> ModifyTransaccionDelegada(TransaccionDelegadaDTO transaccionDelegadaDTO, ModifyTransaccionDelegadaDelegate metodoModifyTransaccion)
        {
            IActionResult result;
            User user;
            Operacion operacion;
            TransaccionDelegada transaccionDelegada;

            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
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

                        result = metodoModifyTransaccion(transaccionDelegada, transaccionDelegadaDTO);
                        if (result is OkResult)
                            await Context.SaveChangesAsync();

                    }
                    else result = Forbid();
                }

            }
            else result = Forbid();
            return result;
        }



    }
}
