using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BancDelTemps.ApiRest.Models
{
    public class Operacion
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        public bool Completada { get; set; }
        public long? RevisorId { get; set; }
        [JsonIgnore]
        public User Revisor { get; set; }
        [NotMapped]
        [JsonIgnore]
        public bool IsRevisada => RevisorId.HasValue;
        /// <summary>
        /// Quien revisa ha aprobado la operación
        /// </summary>
        public bool IsValid { get; set; }
        [NotMapped]
        [JsonIgnore]
        public bool IsOk => IsRevisada && IsValid;
    
    }
}
