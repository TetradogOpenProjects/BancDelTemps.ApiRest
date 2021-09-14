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
        IConfiguration Configuration { get; set; }
        public IHttpContext ContextoHttp { get; set; }
        public GiftController(Context context, IConfiguration configuration)
        {
            Context = context;
            Configuration = configuration;
            ContextoHttp = new ContextoHttp(HttpContext);
        }

        [HttpGet("")]
        public async Task<IActionResult> GetAll(){
            IActionResult result;
            User user;
            if (ContextoHttp.IsAuthenticated)
            {
                user = await Context.Users.FindAsync(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (Equals(user, default))
                {
                    result = NotFound();//No se si es posible que se de pero lo pondré por si acaso de momento.
                }
                else
                {
                    result = Ok(new GiftsDTO(user, Context));
                }
                
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
    }
}