using Gabriel.Cat.S.Extension;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
        public static void RemoveFieldAutoAdded<T>(this  ModelBuilder modelBuilder, T context) where T : DbContext
        {
            modelBuilder.RemoveFieldAutoAdded(context.GetType().GetPropiedadesTipo().Where(p => p.Tipo.IsGenericType && p.Tipo.GetGenericTypeDefinition().Equals(typeof(DbSet<>))).Select(p => p.Tipo.GetGenericArguments()[0]).ToArray());
        }
        public static void RemoveFieldAutoAdded(this ModelBuilder modelBuilder,params Type[] typesContext)
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
}
