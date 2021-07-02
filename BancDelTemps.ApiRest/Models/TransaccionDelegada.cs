using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancDelTemps.ApiRest.Models
{
    public class TransaccionDelegada
    {
        public int Id { get; set; }
        public Guid OperacionId { get; set; }
        public Operacion Operacion { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int? TransaccionId { get; set; }
        public Transaccion Transaccion { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime? Fin { get; set; }
        public bool IsActiva => DateTime.UtcNow > Inicio && (!Fin.HasValue || DateTime.UtcNow < Fin.Value);

    }
}
