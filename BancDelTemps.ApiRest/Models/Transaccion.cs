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


        public int UserFromId { get; set; }
        public User UserFrom { get; set; }
        public int UserToId { get; set; }
        public User UserTo { get; set; }
        public int Minutos { get; set; }
        public DateTime Fecha { get; set; }
        public int? UserValidatorId { get; set; }
        public User UserValidator { get; set; }
        //así se puede delegar el hacer la transacción por otra persona
        public int? UserFromSignedId { get; set; }
        public User UserFromSigned { get; set; }
        [NotMapped]
        public bool IsValidated => UserValidatorId.HasValue;
    }
}
