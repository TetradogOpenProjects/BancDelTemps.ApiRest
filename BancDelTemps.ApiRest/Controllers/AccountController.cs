using BancDelTemps.ApiRest.Models;
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
        public AccountController(Context context,IConfiguration configuration)
        {
            Context = context;
            Configuration = configuration;
        }

        [HttpGet]
        [Route("")]
        [Authorize]
        public IActionResult GetUser()
        {
            IActionResult result;
            User user;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                user = Context.GetUser(Models.User.GetEmailFromHttpContext(HttpContext));
                result = Ok(new UserDTO(user));
            }
            else result = Unauthorized();
            return result;
        }
        [HttpGet]
        [Route("login")]
        public IActionResult GoogleLogin()
        {
            AuthenticationProperties properties;
            properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(GetToken)) };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        [Route("token")]
        public async Task<IActionResult> GetToken()
        {
            IActionResult result;
            User userInfoAux;
            JwtSecurityToken token;
            AuthenticateResult googleResult;

            googleResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (googleResult.Succeeded)
            {
                userInfoAux = new User(googleResult.Principal);
                if (!Context.ExistUser(userInfoAux))
                {
                    //no existe pues lo añado
                    try
                    {
                        Context.Users.Add(userInfoAux);
                        await Context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        throw;
                    }
                }

                token = userInfoAux.GetToken(Configuration);
                result = Ok(token.WriteToken());
            }
            else
            {
                result = Unauthorized();
            }
            return result;


        }
    }
}
