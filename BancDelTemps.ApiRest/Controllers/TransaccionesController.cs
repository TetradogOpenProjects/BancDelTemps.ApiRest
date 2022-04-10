using BancDelTemps.ApiRest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace BancDelTemps.ApiRest.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json", new string[] { "text/plain", "text/json" })]
    public class TransaccionesController : Controller
    {
        delegate Task<IActionResult> ModifyTransaccionDelegate(Transaccion transaccion, TransaccionDTO transaccionDTO);
        delegate IActionResult ModifyTransaccionDelegadaDelegate(TransaccionDelegada transaccion, TransaccionDelegadaDTO transaccionDTO);

        Context Context { get; set; }
        public IHttpContext ContextoHttp { get; set; }
        public TransaccionesController(Context context)
        {
            Context = context;
            ContextoHttp = new ContextoHttp(HttpContext);
        }

        [HttpGet("{idTransaccion:long}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransaccionDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        [SwaggerOperation($"Se usa para obetener la información de una transacción, lo puede usar el usuario que envia, recibe o un '{Permiso.MODTRANSACCION} / {Permiso.ADMIN}'")]
        public async Task<IActionResult> Get(long idTransaccion)
        {

            IActionResult result;
            User user;
            Transaccion transaccion;
            if (ContextoHttp.IsAuthenticated)
            {
                user = await Context.Users.FindAsync(Models.User.GetEmailFromHttpContext(ContextoHttp));
                transaccion = await Context.Transacciones.FindAsync(idTransaccion);

                if (Equals(transaccion, default))
                {
                    result = NotFound();
                }
                else if (transaccion.UserFromId == user.Id || transaccion.UserToId == user.Id || user.IsModTransaccion)
                {
                    result = Ok(new TransaccionDTO(transaccion));
                }
                else result = Unauthorized();

            }
            else result = this.NotLoggedIn();

            return result;
        }
        [HttpGet("Delegar/{idTransaccionDelegada:long}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransaccionDelegadaDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        [SwaggerOperation($"Se usa para obetener la transaccion delegada, lo puede usar quien la ha delegado y  un '{Permiso.MODTRANSACCION}/{Permiso.ADMIN}'")]
        public async Task<IActionResult> GetDelegada(long idTransaccionDelegada)
        {
            IActionResult result;
            User user;
            TransaccionDelegada transaccionDelegada;
            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermisoWithTransacciones(Models.User.GetEmailFromHttpContext(ContextoHttp));
                transaccionDelegada = await Context.TransaccionesDelegadas.Where(t => t.Id == idTransaccionDelegada).FirstOrDefaultAsync();
                if (Equals(transaccionDelegada, default))
                {
                    result = NotFound();
                }
                else if (transaccionDelegada.UserId != user.Id && !user.IsModTransaccion)
                {
                    result = Unauthorized();
                }
                else
                {
                    result = Ok(new TransaccionDelegadaDTO(transaccionDelegada));

                }
            }
            else result = this.NotLoggedIn();

            return result;
        }

        [HttpGet("All/{ticksLastUpdate:long}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransaccionesDTO))]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        [SwaggerOperation($"Se usa para obetener las transacciones'")]
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransaccionDelegadaDTO[]))]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
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
                result = this.NotLoggedIn();
            }
            return result;
        }

        [HttpGet("User/{userId:long}/{ticksLastUpdate:long}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransaccionesDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        [SwaggerOperation($"Se usa para obetener todas las transacciones de un usuario,solo lo puede usar  un ' {Permiso.ADMIN}'")]
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
            else result = this.NotLoggedIn();
            return result;
        }

        [HttpGet("User/Delegadas/{userId:long}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransaccionDelegadaDTO[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        [SwaggerOperation($"Se usa para obetener todas las transacciones delegadas de un usuario, solo lo puede usar un '{Permiso.ADMIN}'")]
        public IActionResult GetAllDelegadasUser(long userId)
        {
            IActionResult result;
            User admin;
            if (ContextoHttp.IsAuthenticated)
            {
                admin = Context.GetUserPermisoWithTransacciones(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (admin.IsAdmin)
                {
                    result = Ok(Context.GetUserPermisoWithTransacciones(userId).TransaccionesSigned.Select(t => new TransaccionDelegadaDTO(t)));
                }
                else result = Unauthorized();

            }
            else
            {
                result = this.NotLoggedIn();
            }
            return result;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransaccionesGrupoDTO))]
        [ProducesResponseType(StatusCodes.Status206PartialContent, Type = typeof(TransaccionDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerResponse(OwnStatusCodes.NotValidated, OwnMessage.NotValidated, typeof(ContentResult))]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        [SwaggerOperation($"Se usa para crear una transacción, lo puede usar el usuario que hizo la operación o un '{Permiso.MODTRANSACCION} / {Permiso.ADMIN}'")]
        public async Task<IActionResult> AddTransaccion(TransaccionDTO transaccionDTO)
        {
            IActionResult result;
            User user;

            Operacion operacion;
            IEnumerable<Transaccion> transacciones;
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
                    else
                    {
                        if (operacion.UserId != user.Id && !user.IsModTransaccion)
                        {
                            result = Unauthorized();//si la operación no la ha creado él o no es un mod no puede hacerlo
                        }
                        else if (operacion.Completada)
                        {
                            transacciones = Context.Transacciones.Where(t => t.OperacionId == operacion.Id);
                            if (transacciones.Any())
                            {
                                result = Ok(new TransaccionesGrupoDTO(transacciones));
                            }
                            else
                            {//si hay más participantes a dar el pago y se quiere añadir a posteriori
                                result = await DoTransaccion(transaccionDTO, user, operacion);
                            }
                        }
                        else
                        {
                            result = await DoTransaccion(transaccionDTO, user, operacion);

                        }
                    }
                }
                else result = this.NotValidated();//no se puede trabajar con alguien no validado!

            }
            else result = this.NotLoggedIn();
            return result;
        }
        async Task<IActionResult> DoTransaccion([NotNull] TransaccionDTO transaccionDTO, User user, Operacion operacion)
        {
            IActionResult result;

            TransaccionDelegada transaccionDelegada;

            bool autoritzed;


            if (operacion.Id != transaccionDTO.IdOperacion)
            {
                result = BadRequest($"la {nameof(Operacion)} con {nameof(Operacion.Id)}={transaccionDTO.IdOperacion} no es la misma que la enviada {nameof(Operacion.Id)}={operacion.Id}");
            }
            else
            {
                autoritzed = user.Id == transaccionDTO.IdFrom || user.IsModTransaccion;

                if (!autoritzed)
                {
                    transaccionDelegada = Context.GetTransaccionDelegada(operacion);

                    autoritzed = !Equals(transaccionDelegada, default)
                                 && user.Id == transaccionDelegada.UserId
                                 && transaccionDelegada.IsActiva;


                }

                if (autoritzed)
                {
                    result = await DoTransaccion(transaccionDTO, operacion: operacion, userFrom: user);
                    if (result is OkObjectResult)
                    {
                        transaccionDTO.Id = ((result as OkObjectResult).Value as Transaccion).Id;
                        result = StatusCode(StatusCodes.Status206PartialContent, transaccionDTO);
                    }
                }

                else
                {

                    result = Unauthorized();


                }
            }
            return result;
        }
        internal async Task<IActionResult> DoTransaccion([NotNull] TransaccionDTO transaccionDTO, User mod = default, Operacion operacion = default, User userFrom = default)
        {
            IActionResult result = default;
            Transaccion transaccion = default;

            #region Validar Parametros
            if (Equals(operacion, default))
            {
                operacion = await Context.Operaciones.FindAsync(transaccionDTO.IdOperacion);
            }
            else if (transaccionDTO.IdOperacion != operacion.Id)
            {
                result = BadRequest($"La {nameof(Operacion)} '{transaccionDTO.IdOperacion}' no coincide con la enviada '{operacion.Id}'");
            }
            if (Equals(userFrom, default))
            {
                userFrom = Context.GetUserPermisoWithTransacciones(transaccionDTO.IdFrom);
            }
            else if (transaccionDTO.IdFrom != userFrom.Id)
            {
                result = BadRequest($"El {nameof(Models.User)} '{await Context.Users.FindAsync(transaccionDTO.IdFrom)}' no coincide con el enviado '{userFrom}'");
            }
            if (Equals(mod, default))
            {
                mod = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (!mod.IsModTransaccion)
                {
                    mod = default;
                }
            }
            else if (!mod.IsModTransaccion)
            {
                result = BadRequest($"El {nameof(Models.User)} {nameof(mod)}={mod} no tiene el permiso '{Permiso.MODTRANSACCION}' ({nameof(mod.IsModTransaccion)})");
            }
            #endregion
            if (Equals(result, default))
            {
                if (userFrom.TiempoDisponible - transaccionDTO.Minutos >= 0)
                {
                    //aqui pongo los criterios para poder hacer la operacion
                    //si se puede hacer creo la transacción
                    if (!operacion.IsRevisada || !Equals(mod, default))
                    {
                        operacion.Completada = transaccionDTO.IsComplete;
                        Context.Operaciones.Update(operacion);
                        transaccion = transaccionDTO.ToTransaccion();
                        if (!Equals(mod, default))
                        {
                            transaccion.UserValidator = mod;
                        }
                        await Context.Transacciones.AddAsync(transaccion);
                        await Context.SaveChangesAsync();
                    }
                }
                result = Ok(transaccion);
            }
            return result;
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerResponse(OwnStatusCodes.NotValidated, OwnMessage.NotValidated, typeof(ContentResult))]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        [SwaggerOperation($"Se usa para actualizar una transacción, lo puede usar el usuario que la hizo/quién lo tiene delegado (solo si no ha pasado el plazo máximo para hacerlo)  o un '{Permiso.MODTRANSACCION} / {Permiso.ADMIN}' que puede siempre.")]

        public async Task<IActionResult> UpdateTransaccion(TransaccionDTO transaccionDTO)
        {
            return await ModifyTransaccion(transaccionDTO, async (transaccion, tDTO) =>
             {
                 IActionResult result;
                 User user = Context.Users.Find(tDTO.IdTo);
                 if (Equals(user, default))
                 {
                     result = NotFound();
                 }
                 else if (!user.IsValidated)
                 {
                     result = this.NotValidated();
                 }
                 else
                 {
                    
                     result = await DoTransaccionUpdate(tDTO, default, transaccion);
                 }
                 return result;
             });
        }

        internal async Task<IActionResult> DoTransaccionUpdate([NotNull] TransaccionDTO tDTO, User mod = default, Transaccion transaccion = default, User userFrom = default, Operacion operacion = default)
        {
            IActionResult result = default;
            bool puedeHacerla;

            #region Validar Parametros
            if (Equals(transaccion, default))
            {
                transaccion = await Context.Transacciones.FindAsync(tDTO.Id);
                if (Equals(transaccion, default))
                    throw new Exception($"{nameof(Transaccion)} {tDTO.Id} no encontrada!!");
            }
            else if (tDTO.Id != transaccion.Id)
            {
                throw new Exception($"La {nameof(Transaccion)} '{tDTO.Id}' no se corresponde con la enviada '{transaccion.Id}'");
            }
            if (Equals(operacion, default))
            {
                operacion = await Context.Operaciones.FindAsync(tDTO.IdOperacion);
                if (Equals(operacion, default))
                    throw new Exception($"{nameof(Operacion)} {tDTO.IdOperacion} no encontrada!!");
            }
            else if (tDTO.IdOperacion != operacion.Id)
            {
                throw new Exception($"La {nameof(Operacion)} '{tDTO.Id}' no se corresponde con la enviada '{transaccion.Id}'");
            }
            if (Equals(userFrom, default))
            {
                userFrom = Context.GetUserPermisoWithTransacciones(tDTO.IdFrom);

            }
            else if (tDTO.IdFrom != userFrom.Id)
            {
                throw new Exception($"El {nameof(Models.User)} {userFrom} no se corresponde con el enviado {await Context.Users.FindAsync(tDTO.IdFrom)}");
            }
            #endregion

            puedeHacerla = tDTO.Minutos <= transaccion.Minutos || userFrom.TiempoDisponible > (tDTO.Minutos - transaccion.Minutos);

            if (puedeHacerla)
            {
                if (Equals(mod, default))
                {
                    mod = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                    if (!mod.IsModTransaccion)
                    {
                        mod = default;
                    }
                }
                else if (!mod.IsModTransaccion)
                {
                    result = BadRequest($"El {nameof(Models.User)} {nameof(mod)}='{mod}' no tiene el permiso '{Permiso.MODTRANSACCION}' ({nameof(mod.IsModTransaccion)})");
                }
                if (Equals(result, default))
                {
                    if (!operacion.IsRevisada || !Equals(mod, default))
                    {
                        transaccion.Minutos = tDTO.Minutos;
                        transaccion.UserToId = tDTO.IdTo;
                        transaccion.Fecha = tDTO.Fecha;
                        if (!Equals(mod, default))
                        {
                            transaccion.UserValidator = mod;

                        }
                        else
                        {//como se ha modificado se tiene que volver a validar!
                            transaccion.UserValidator = default;
                            transaccion.UserValidatorId = default;
                        }

                        Context.Transacciones.Update(transaccion);
                    }
                    else puedeHacerla = false;
                }
            }
            if (Equals(result, default))
                result = Ok(new IsOkDTO(puedeHacerla));
            return result;
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        [SwaggerOperation($"Se usa para eliminar una transacción, lo puede usar el usuario que la hizo/quién lo tiene delegado (solo si no ha pasado el plazo máximo para hacerlo)  o un '{Permiso.MODTRANSACCION} / {Permiso.ADMIN}' que puede siempre.")]
        public async Task<IActionResult> DeleteTransaccion(TransaccionDTO transaccionDTO)
        {
            return await ModifyTransaccion(transaccionDTO, (transaccion, tDTO) =>
            {
                Context.Transacciones.Remove(transaccion);
                return Task.FromResult((IActionResult)Ok());

            });

        }

        async Task<IActionResult> ModifyTransaccion(TransaccionDTO transaccionDTO, [NotNull] ModifyTransaccionDelegate metodoModifyTransaccion)
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
                                result = await metodoModifyTransaccion(transaccion, transaccionDTO);
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
            else result = this.NotLoggedIn();


            return result;
        }



        [HttpPost("Delegar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerResponse(OwnStatusCodes.NotValidated, OwnMessage.NotValidated, typeof(ContentResult))]
        [SwaggerResponse(OwnStatusCodes.OperacionAcabada, OwnMessage.OperacionAcabada, typeof(ContentResult))]
        [SwaggerResponse(OwnStatusCodes.OperacionRepetida, OwnMessage.OperacionRepetida, typeof(ContentResult))]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        [SwaggerOperation($"Se usa para delegar una transacción, lo puede usar el usuario que hizo la operación o un '{Permiso.MODTRANSACCION} / {Permiso.ADMIN}'")]
        public async Task<IActionResult> AddTransaccionDelegada(TransaccionDelegadaDTO transaccionDelegadaDTO)
        {
            IActionResult result;
            User userFrom;
            Operacion operacion;
            User userDelegado;
            TransaccionDelegada transaccionDelegada;

            if (ContextoHttp.IsAuthenticated)
            {
                if (Equals(transaccionDelegadaDTO, default))
                {
                    result = BadRequest();
                }
                else
                {
                    userFrom = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                    operacion = await Context.Operaciones.FindAsync(transaccionDelegadaDTO.IdOperacion);
                    if (Equals(operacion, default))
                    {
                        result = NotFound();
                    }
                    else if (operacion.Completada)
                    {
                        result = this.OperacionAcabada();//otro error para decir que ya se ha terminado
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
                                    result = this.OperacionRepetida();//otro error para decir que ya se ha delegado
                                }
                            }
                            else result = this.NotValidated();
                        }
                        else result = NotFound();

                    }
                    else
                    {
                        result = Unauthorized();
                    }
                }
            }
            else result = this.NotLoggedIn();

            return result;
        }

        [HttpPut("Delegar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ParamDTO))]
        [SwaggerResponse(OwnStatusCodes.NotValidated, OwnMessage.NotValidated, typeof(ContentResult))]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        [SwaggerOperation($"Se usa para actualizar la transacción delegada, lo puede usar el usuario que hizo la operación o un '{Permiso.MODTRANSACCION} / {Permiso.ADMIN}'")]
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
                    result = this.NotValidated();
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ParamDTO))]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        [SwaggerOperation($"Se usa para dejar de delegar una transacción, lo puede usar el usuario que hizo la operación o un '{Permiso.MODTRANSACCION} / {Permiso.ADMIN}'")]
        public async Task<IActionResult> DeleteTransaccionDelegada(TransaccionDelegadaDTO transaccionDelegadaDTO)
        {
            return await ModifyTransaccionDelegada(transaccionDelegadaDTO, (transaccionDelegada, tDTO) =>
            {
                Context.TransaccionesDelegadas.Remove(transaccionDelegada);
                return Ok();
            });
        }

        async Task<IActionResult> ModifyTransaccionDelegada(TransaccionDelegadaDTO transaccionDelegadaDTO, [NotNull] ModifyTransaccionDelegadaDelegate metodoModifyTransaccion)
        {
            IActionResult result;
            User user;
            Operacion operacion;
            TransaccionDelegada transaccionDelegada;

            if (ContextoHttp.IsAuthenticated)
            {
                if (Equals(transaccionDelegadaDTO, default))
                {
                    result = BadRequest();
                }
                else
                {
                    user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                    transaccionDelegada = await Context.TransaccionesDelegadas.Where(t => t.OperacionId.Equals(transaccionDelegadaDTO.IdOperacion)).FirstOrDefaultAsync();

                    if (Equals(transaccionDelegada, default))
                    {
                        result = NotFound(new ParamDTO(nameof(TransaccionDelegada)));
                    }
                    else
                    {
                        operacion = await Context.Operaciones.FindAsync(transaccionDelegadaDTO.IdOperacion);
                        if (Equals(operacion, default))
                        {
                            result = NotFound(new ParamDTO(nameof(Operacion)));
                        }
                        else if (user.Id == operacion.UserId || user.IsModTransaccion)
                        {

                            result = metodoModifyTransaccion(transaccionDelegada, transaccionDelegadaDTO);
                            if (result is OkResult)
                                await Context.SaveChangesAsync();

                        }
                        else result = Unauthorized();
                    }
                }

            }
            else result = this.NotLoggedIn();
            return result;
        }



    }
}
