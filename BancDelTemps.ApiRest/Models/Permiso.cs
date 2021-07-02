﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace BancDelTemps.ApiRest.Models
{
    [Index(nameof(Nombre), IsUnique = true, Name = nameof(Nombre) + nameof(Permiso) + "_uniqueContraint")]
    public class Permiso
    {
        public const string ADMIN = "admin";

        public const string MODTRANSACCION = "mod transaccion";

        public static string[] Todos =>new string[]{ADMIN,MODTRANSACCION};

        public int Id { get; set; }
        public string Nombre { get; set; }
        public ICollection<UserPermiso> Users { get; set; }
    }

}
