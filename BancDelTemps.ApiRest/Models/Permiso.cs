using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace BancDelTemps.ApiRest.Models
{
    [Index(nameof(Nombre), IsUnique = true, Name = nameof(Nombre) + nameof(Permiso) + "_uniqueContraint")]
    public class Permiso
    {
        //no se escriben con con mayúsculas!!
        public const string ADMIN = "admin";
        public const string MODTRANSACCION = "mod transaccion";
        public const string MODVALIDATION = "mod validation";
        public const string CANLISTUSER = "can list users";
        public const string MODGIFT = "mod gift";
        public const string MODOPERACION = "mod operacion";
        public static string[] Todos =>new string[]{ADMIN,MODTRANSACCION,MODVALIDATION,CANLISTUSER,MODGIFT,MODOPERACION};

        public int Id { get; set; }
        public string Nombre { get; set; }
        public ICollection<UserPermiso> Users { get; set; }
    }

}
