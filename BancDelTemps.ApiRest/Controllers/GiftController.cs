using BancDelTemps.ApiRest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.Annotations;
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
    [Produces("application/json", new string[] { "text/plain", "text/json" })]
    public class GiftController : Controller
    {
        Context Context { get; set; }
        TransaccionesController TransaccionesController { get; set; }
        public IHttpContext ContextoHttp { get; set; }
        public GiftController(Context context)
        {
            Context = context;
            ContextoHttp = new ContextoHttp(HttpContext);
            TransaccionesController = new TransaccionesController(Context);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(GiftsDTO))]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> GetAll()
        {
            IActionResult result;
            User user;
            if (ContextoHttp.IsAuthenticated)
            {
                user = await Context.Users.FindAsync(Models.User.GetEmailFromHttpContext(ContextoHttp));
                result = Ok(new GiftsDTO(user, Context));
            }
            else result = this.NotLoggedIn();
            return result;

        }
        [HttpGet("User/{userId:long}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GiftsDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> GetAllUser(long userId)
        {
            IActionResult result;
            User validador;
            if (ContextoHttp.IsAuthenticated)
            {
                validador = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (validador.IsModGift)
                {
                    if (Equals(await Context.Users.FindAsync(userId), default))
                    {
                        result = NotFound();
                    }
                    else
                    {
                        result = Ok(new GiftsDTO(userId, Context));
                    }
                }
                else result = Unauthorized();
            }
            else result = this.NotLoggedIn();
            return result;
        }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GiftDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> AddGift(TransaccionDTO transaccionDTO)
        {
            User user;
            Transaccion transaccion;
            Gift gift;
            IActionResult result;


            if (ContextoHttp.IsAuthenticated)
            {
                if (Equals(transaccionDTO, default))
                {
                    result = BadRequest();
                }
                else
                {
                    user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                    if (user.Id == transaccionDTO.IdFrom || user.IsModGift)
                    {
   
                        result = await TransaccionesController.DoTransaccion(transaccionDTO, user.IsModGift && user.IsModTransaccion ? user : default);
                        if (result is OkObjectResult)
                        {
                            gift = new Gift()
                            {
                                TransaccionId = ((result as OkObjectResult).Value as Transaccion).Id
                            };
                            Context.Gifts.Add(gift);
                            result = Ok(new GiftDTO(gift));
                        }
    

                    }
                    else result = Unauthorized();
                }
            }
            else result = this.NotLoggedIn();

            return result;
        }
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> UpdateGift(TransaccionDTO transaccionDTO)
        {
            IActionResult result;
            User user;
            
            if (ContextoHttp.IsAuthenticated)
            {
                if (Equals(transaccionDTO, default))
                {
                    result = BadRequest();
                }
                else
                {
                    user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                    if (user.Id == transaccionDTO.IdFrom || user.IsModGift)
                    {
                        if (Context.Gifts.Where(g => g.TransaccionId == transaccionDTO.Id).Any())
                        {
                            result = await TransaccionesController.DoTransaccionUpdate(transaccionDTO, user.IsModGift && user.IsModTransaccion ? user : default);
                            if (result is OkObjectResult)
                            {
                                if ((bool)(result as OkObjectResult).Value)
                                {
                                    result = Ok();
                                }
                                else
                                {
                                    result = Forbid();
                                }
                            }
                        }                        
                        else
                        {
                            result = NotFound();
                        }
                    }
                    else result = Unauthorized();
                }
            }
            else result = this.NotLoggedIn();

            return result;
        }
    }
}