using BancDelTemps.ApiRest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancDelTemps.ApiRest.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class OperationController : ControllerBase
    {
        Context Context { get; set; }
        public IHttpContext ContextoHttp { get; set; }
        public OperationController(Context context)
        {
            Context = context;
            ContextoHttp = new ContextoHttp(HttpContext);
        }
        [HttpGet("{idOperacion:long}")]
        [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(OperacionDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Get(long idOperacion)
        {
            IActionResult result;
            User user;
            Operacion operacion;

            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                operacion = await Context.Operaciones.FindAsync(idOperacion);
                if (Equals(operacion, default))
                {
                    result = NotFound();
                }
                else if (operacion.UserId == user.Id || user.IsModOperacion)
                {
                    result = Ok(new OperacionDTO(operacion));
                }
                else result = Unauthorized();
            }
            else result = Forbid();
            return result;

        }
        [HttpGet("All/{ticksLastUpdate:long}")]
        [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(OperacionDTO[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetToValidateAll(long ticksLastUpdate)
        {
            IActionResult result;
            User validador;
            if (ContextoHttp.IsAuthenticated)
            {
                validador = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (validador.IsModOperacion)
                {
                    result = Ok(Context.Operaciones
                                       .Where(o => !o.IsRevisada && o.Completada && o.Fecha.Ticks > ticksLastUpdate)
                                       .Select(o => new OperacionDTO(o))

                            );
                }
                else result = Unauthorized();
            }
            else result = Forbid();
            return result;

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(OperacionDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddOperation(OperacionDTO operacionDTO)
        {
            IActionResult result;
            User user;
            Operacion operacion;
            if (ContextoHttp.IsAuthenticated)
            {
                if (Equals(operacionDTO, default))
                {
                    result = BadRequest();
                }
                else
                {
                    user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                    if (Equals(await Context.Users.FindAsync(operacionDTO.UserId), default))
                    {
                        result = NotFound();
                    }
                    else if (user.Id == operacionDTO.UserId || user.IsModOperacion)
                    {
                        operacion = new Operacion()
                        {
                            UserId = operacionDTO.UserId
                        };
                        if (user.IsModOperacion)
                        {
                            //si es un mod lo autovalido
                            operacion.Revisor = user;
                            operacion.IsValid = true;
                        }
                        Context.Operaciones.Add(operacion);
                        await Context.SaveChangesAsync();
                        result = Ok(new OperacionDTO(operacion));
                    }
                    else result = Unauthorized();
                }
            }
            else result = Forbid();
            return result;
        }
        [HttpPost("{idOpracion:long}/{isValid:bool}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OperacionDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Validate(long idOperacion, bool isValid)
        {
            return await DoValidate(idOperacion, isValid);
        }
        [HttpPut("{idOpracion:long}/{isValid:bool}")]
        [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(OperacionDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]        
        public async Task<IActionResult> Revalidate(long idOperacion, bool isValid)
        {
            return await DoValidate(idOperacion, isValid, true);
        }

        async Task<IActionResult> DoValidate(long idOperacion, bool isValid, bool force = false)
        {
            ActionResult result;
            User validador;
            Operacion operacion;
            if (ContextoHttp.IsAuthenticated)
            {
                validador = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (validador.IsModOperacion)
                {
                    operacion = await Context.Operaciones.FindAsync(idOperacion);
                    if (Equals(operacion, default))
                    {
                        result = NotFound();
                    }
                    else
                    {
                        if (operacion.Completada)
                        {
                            if (!operacion.IsRevisada || force)
                            {
                                operacion.Revisor = validador;
                                operacion.IsValid = isValid;
                                await Context.SaveChangesAsync();
                            }
                            result = Ok(new OperacionDTO(operacion));
                        }
                        else result = Forbid();
                    }
                }
                else result = Unauthorized();
            }
            else result = Forbid();
            return result;
        }

        [HttpDelete("{idOperacion:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(long idOperacion)
        {
            ActionResult result;
            User mod;
            Operacion operacion;
            if (ContextoHttp.IsAuthenticated)
            {
                mod = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (mod.IsModOperacion)
                {
                    operacion = await Context.Operaciones.FindAsync(idOperacion);
                    if (Equals(operacion, default))
                    {
                        result = NotFound();
                    }
                    else
                    {
                        //miro que no haya nada que use el idOperacion
                        if (Context.Transacciones.Where(t => t.OperacionId == operacion.Id).Any())
                        {
                            result = Forbid();
                        }
                        else
                        {
                            //si hay más que usen las operaciones se ponen más else if o se pone en la condición un OR ya se verá
                            Context.Operaciones.Remove(operacion);
                            await Context.SaveChangesAsync();
                            result = Ok();
                        }
                    }
                }
                else result = Unauthorized();

            }
            else result = Forbid();
            return result;
        }
    }
}
