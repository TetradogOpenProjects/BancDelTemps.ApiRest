using BancDelTemps.ApiRest.Models;
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
        delegate void ModifyTransaccionDelegate(Transaccion transaccion, TransaccionDTO transaccionDTO);
        Context Context { get; set; }
        public TransaccionesController(Context context) => Context = context;

        [HttpGet]
        [Route("")]
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
        [HttpGet]
        [Route("Delegadas")]
        public IActionResult GetAllDelegadas()
        {
            IActionResult result;
            User user;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                user = Context.GetUserPermisoWithTransacciones(Models.User.GetEmailFromHttpContext(HttpContext));
                result = Ok(user.TransaccionesSigned);
            }
            else
            {
                result = Forbid();
            }
            return result;
        }
        [HttpGet]
        [Route("{userId:int}")]
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
        [HttpGet]
        [Route("Delegadas/{userId:int}")]
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
            else result = Forbid();
            return result;
        }
        [HttpPut("")]
        public async Task<IActionResult> UpdateTransaccion(TransaccionDTO transaccionDTO)
        {
            return await ModifyTransaccion(transaccionDTO, (transaccion, tDTO) =>
            {
                transaccion.Minutos = tDTO.Minutos;
                transaccion.UserToId = tDTO.IdTo;
                transaccion.Fecha = tDTO.Fecha;
                transaccion.UserValidator = default;
                transaccion.UserValidatorId = default;
                Context.Transacciones.Update(transaccion);
            });
        }
        [HttpDelete("")]
        public async Task<IActionResult> DeleteTransaccion(TransaccionDTO transaccionDTO)
        {
            return await ModifyTransaccion(transaccionDTO, (transaccion, tDTO) =>
            {
                Context.Transacciones.Remove(transaccion);
            });

        }

        async Task<IActionResult> ModifyTransaccion(TransaccionDTO transaccionDTO, ModifyTransaccionDelegate modifyTransaccion)
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
                            modifyTransaccion(transaccion, transaccionDTO);
                            await Context.SaveChangesAsync();
                            result = Ok();
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
                } else if (userFrom.Id == operacion.UserId || userFrom.IsModTransaccion)
                {

                    if (Context.ExistUser(transaccionDelegadaDTO.IdUsuarioADelegar))
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
                        }else
                        {
                            //la operacion ya se ha delegado
                            result =BadRequest();
                        }
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
    }
}
