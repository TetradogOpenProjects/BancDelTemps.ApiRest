using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BancDelTemps.ApiRest.Models
{
    public class Transaccion
    {
        public static TimeSpan MaxBackTime{get;set;}=TimeSpan.FromDays(15);//poner el numero de dias que sea adecuado

        public long Id { get; set; }
        public long OperacionId { get; set; }
        public Operacion Operacion { get; set; }
        public long UserFromId { get; set; }
        public User UserFrom { get; set; }
        public long UserToId { get; set; }
        public User UserTo { get; set; }
        public int Minutos { get; set; }
        public DateTime Fecha { get; set; }
        public long? UserValidatorId { get; set; }
        public User UserValidator { get; set; }
        [NotMapped]
        public bool IsValidated => UserValidatorId.HasValue;
        [NotMapped]
        public bool CanBack=> (DateTime.Now.Utc - Fecha ) < MaxBackTime; 
    }
}
