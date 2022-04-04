using BancDelTemps.ApiRest.Models;
using System.Collections.Generic;
using System.Linq;

namespace BancDelTemps.ApiRest
{
    public static class DataSeeder
    {  
        public static void SeedAll(this Context context)
        {
            context.SeedPermisos();
        }
        public static void SeedPermisos(this Context context)
        {
            List<Permiso> permisos;
            context.Database.EnsureCreated();

            if (!context.Permisos.Any())
            {
                permisos = new List<Permiso>();
                for (int i = 0; i < Permiso.Todos.Length; i++)
                {
                   permisos.Add(new Permiso { Nombre = Permiso.Todos[i] });
                }
                context.Permisos.AddRange(permisos);
                context.SaveChanges();
            }
        }
    

        
    }
}
