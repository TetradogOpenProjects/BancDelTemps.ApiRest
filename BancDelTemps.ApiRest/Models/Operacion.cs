using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BancDelTemps.ApiRest.Models
{
    public class Operacion
    {
        public long Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
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
    public class OperacionDTO
    {
        public OperacionDTO() { }
        public OperacionDTO(Operacion operacion)
        {
            Id = operacion.Id;
            UserId = operacion.UserId;
            Date = operacion.Fecha;
            IsCompleted = operacion.Completada;
            RevisorId = operacion.RevisorId;
            IsValid = operacion.IsValid;

        }
        public long? Id { get; set; }
        public long UserId { get; set; }
        public DateTime? Date { get; set; }
        public bool IsCompleted { get; set; }
        public long? RevisorId { get; set; }
        public bool IsValid { get; set; }
    }
}
