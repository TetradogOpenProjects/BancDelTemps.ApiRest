using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BancDelTemps.ApiRest.Models
{
    public class Operacion
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public bool Completada { get; set; }
        public long? RevisorId { get; set; }
        public User Revisor { get; set; }
        [NotMapped]
        public bool IsRevisada => RevisorId.HasValue;
        /// <summary>
        /// Quien revisa ha aprobado la operación
        /// </summary>
        public bool IsValid { get; set; }
        [NotMapped]
        public bool IsOk => IsRevisada && IsValid;
    
    }
}
