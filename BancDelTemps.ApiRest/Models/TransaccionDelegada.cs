using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BancDelTemps.ApiRest.Models
{
    public class TransaccionDelegada
    {
        public long Id { get; set; }
        public long OperacionId { get; set; }
        public Operacion Operacion { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public long? TransaccionId { get; set; }
        public Transaccion Transaccion { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime? Fin { get; set; }
        public bool IsActiva => DateTime.UtcNow > Inicio && (!Fin.HasValue || DateTime.UtcNow < Fin.Value);

    }
}
