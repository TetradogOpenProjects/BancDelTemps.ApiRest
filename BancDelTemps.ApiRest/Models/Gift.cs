using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace BancDelTemps.ApiRest.Models
{
    public class Gift{
        public long Id{get;set;}
        public long TransaccionId{get;set;}
        public Transaccion Transaccion{get;set;}
    }
}