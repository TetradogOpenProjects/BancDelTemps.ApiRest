using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BancDelTemps.ApiRest.Models
{
    public class Transaccion
    {
        public int Id { get; set; }
        public Guid OperacionId { get; set; }
        public Operacion Operacion { get; set; }

        public int UserFromId { get; set; }
        public User UserFrom { get; set; }
        public int UserToId { get; set; }
        public User UserTo { get; set; }
        public int Minutos { get; set; }
        public DateTime Fecha { get; set; }
        public int? UserValidatorId { get; set; }
        public User UserValidator { get; set; }
        //así se puede delegar el hacer la transacción por otra persona
        public int? TransaccionDelegadaId { get; set; }
        public TransaccionDelegada TransaccionDelegada { get; set; }
        [NotMapped]
        public bool IsValidated => UserValidatorId.HasValue;
    }

    public class Operacion
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public bool Completada { get; set; }
        public int? RevisadorId { get; set; }
        public User Revisor { get; set; }
        public bool IsRevisada => RevisadorId.HasValue;
    
    }
}
