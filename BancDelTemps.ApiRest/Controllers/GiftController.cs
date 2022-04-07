using BancDelTemps.ApiRest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        public async Task<IActionResult> GetAll()
        {
            IActionResult result;
            User user;
            if (ContextoHttp.IsAuthenticated)
            {
                user = await Context.Users.FindAsync(Models.User.GetEmailFromHttpContext(ContextoHttp));
                result = Ok(new GiftsDTO(user, Context));
            }
            else result = Forbid();
            return result;

        }
        [HttpGet("User/{userId:long}")]
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
            else result = Forbid();
            return result;
        }
        [HttpPost]
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
   
                        transaccion = await TransaccionesController.DoTransaccion(transaccionDTO, user.IsModGift && user.IsModTransaccion ? user : default);
                        if (!Equals(transaccion, default))
                        {
                            gift = new Gift()
                            {
                                TransaccionId = transaccion.Id
                            };
                            Context.Gifts.Add(gift);
                            result = Ok(gift);
                        }
                        else result = BadRequest();

                    }
                    else result = Unauthorized();
                }
            }
            else result = Forbid();

            return result;
        }
        [HttpPut]
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
                            if (await TransaccionesController.DoTransaccionUpdate(transaccionDTO, user.IsModGift && user.IsModTransaccion ? user : default))
                            {
                                result = Ok();
                            }
                            else
                            {
                                result = Forbid();
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
            else result = Forbid();

            return result;
        }
    }
}