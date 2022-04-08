using Gabriel.Cat.S.Extension;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BancDelTemps.ApiRest
{
    public static class JwtSecurityTokenExtension
    {
        public static string WriteToken(this JwtSecurityToken token)
        {
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
    public static class ModelBuilderExtension
    {
        public static void RemoveFieldAutoAdded<T>(this ModelBuilder modelBuilder, T context) where T : DbContext
        {
            modelBuilder.RemoveFieldAutoAdded(context.GetType().GetPropiedadesTipo().Where(p => p.Tipo.IsGenericType && p.Tipo.GetGenericTypeDefinition().Equals(typeof(DbSet<>))).Select(p => p.Tipo.GetGenericArguments()[0]).ToArray());
        }
        public static void RemoveFieldAutoAdded(this ModelBuilder modelBuilder, params Type[] typesContext)
        {
            IEnumerable<string> properties;
            IMutableEntityType metadata;
            for (int i = 0; i < typesContext.Length; i++)
            {
                metadata = modelBuilder.Entity(typesContext[i].FullName).Metadata;
                properties = metadata.GetDeclaredProperties().Select(p => p.Name).Except(typesContext[i].GetPropiedadesTipo().Select(p => p.Nombre));
                //reviso los modelos para que no tengan campos nuevos
                foreach (string propiedadAQuitar in properties)
                    metadata.RemoveProperty(propiedadAQuitar);
            }
        }
    }
    public static class ControllerExtension
    {
     

        public static IActionResult NotValidated(this ControllerBase controller)
        {
            return GetContent(OwnStatusCodes.NotValidated, OwnMessage.NotValidated);
        }
        public static IActionResult ContentForbbiden(this ControllerBase controller)
        {
            return GetContent(OwnStatusCodes.ContentForbbiden, OwnMessage.ContentForbbien);
        }
        public static IActionResult NotLoggedIn(this ControllerBase controller)
        {
            return GetContent(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn);
       
        }
        public static IActionResult OperacionAcabada(this ControllerBase controller)
        {
            return GetContent(OwnStatusCodes.OperacionAcabada, OwnMessage.OperacionAcabada);
        }
        public static IActionResult OperacionRepetida(this ControllerBase controller)
        {
            return GetContent(OwnStatusCodes.OperacionRepetida, OwnMessage.OperacionRepetida);

        }
        private static ContentResult GetContent(int statusCode, string error)
        {
            return new ContentResult()
            {
                StatusCode = statusCode,
                Content = new OwnMessage(error).ToJson(),
                ContentType = "application/json",
            };
        }
    }

    public class OwnStatusCodes
    {
        public const int NotValidated = 1001;
        public const int ContentForbbiden = NotValidated+1;
        public const int NotLoggedIn = ContentForbbiden+1;
        public const int OperacionAcabada = NotLoggedIn + 1;
        public const int OperacionRepetida = OperacionAcabada + 1;


    }

    public class OwnMessage
    {
        public const string NotLoggedIn = "El usuario no está logueado!";
        public const string ContentForbbien = "El contenido no esta permitido!";
        public const string NotValidated = "El usuario necesita ser validado primero!";
        public const string OperacionAcabada = "Ya se está acabada la operación!";
        public const string OperacionRepetida = "Ya se ha hecho anteriormente esta operación";
        public OwnMessage(string message)
        {
            Message = message;
        }
        public string Message { get; set; }
        
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }

}
