using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BancDelTemps.ApiRest
{
    public interface IHttpContext
    {
        bool IsAuthenticated { get; }
        string[] GetClaimsValueFirstIdentity();
    }
    public class ContextoHttp : IHttpContext
    {
        public ContextoHttp(HttpContext context) => Context = context;

        HttpContext Context { get; set; }
        public bool IsAuthenticated => Context.User.Identity.IsAuthenticated;

        public string[] GetClaimsValueFirstIdentity()
        {
           return Context.User.Identities.FirstOrDefault().Claims.Select(c=>c.Value).ToArray();
        }
    }
}
