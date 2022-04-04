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


            permisos = new List<Permiso>();
            for (int i = 0; i < Permiso.Todos.Length; i++)
            {
                if (!context.Permisos.Any(p => p.Nombre == Permiso.Todos[i]))
                {
                    permisos.Add(new Permiso { Nombre = Permiso.Todos[i] });
                }
            }
            if (permisos.Count > 0)
            {
                context.Permisos.AddRange(permisos);
                context.SaveChanges();
            }

        }



    }
}
