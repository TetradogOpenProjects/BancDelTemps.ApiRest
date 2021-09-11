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
                result = Ok(new GiftsDTO(user,Context));
            }
            else result = Forbid();
            return result;

        }
    }
}