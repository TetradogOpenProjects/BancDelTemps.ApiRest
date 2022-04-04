using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BancDelTemps.ApiRest.Models
{
    public class TransaccionesDTO
    {

        public TransaccionesDTO() { }
        public TransaccionesDTO([NotNull] User user,[NotNull] Context context, long ticksLastUpdate)
        {
            IdUser = user.Id;
            In = user.TransaccionesIn.Where(t => t.Fecha.Ticks > ticksLastUpdate)
                                     .Select(s => new TransaccionDTO(s));

            Out = user.TransaccionesFrom.Where(t => t.Fecha.Ticks > ticksLastUpdate)
                                        .Select(s => new TransaccionDTO(s));

            Signed = user.TransaccionesSigned.Select(s => context.GetTransaccion(s.Operacion))
                                             .Where(t => t.Fecha.Ticks > ticksLastUpdate)
                                             .Select(t => new TransaccionDTO(t));


        }

        public long IdUser { get; set; }
        public IEnumerable<TransaccionDTO> In { get; set; }
        public IEnumerable<TransaccionDTO> Out { get; set; }
        public IEnumerable<TransaccionDTO> Signed { get; set; }

    }
    public class TransaccionesGrupoDTO
    {

        public TransaccionesGrupoDTO([NotNull] IEnumerable<Transaccion> transacciones)
        {
            Others = transacciones.Select(t => new TransaccionDTO(t));
        }
        public IEnumerable<TransaccionDTO> Others { get; set; }
    }

    public class TransaccionDTO
    {

        public TransaccionDTO() { }
        public TransaccionDTO([NotNull] Transaccion transaccion)
        {
            Id = transaccion.Id;
            IdFrom = transaccion.UserFromId;
            IdTo = transaccion.UserToId;
            IsValidated = transaccion.IsValidated;
            Fecha = transaccion.Fecha;
            Minutos = transaccion.Minutos;
            IdOperacion = transaccion.OperacionId;
        }
        public long? Id { get; set; }
        public bool IsComplete { get; set; }
        public long IdFrom { get; set; }
        public long IdTo { get; set; }
        public bool IsValidated { get; set; }
        public long IdOperacion { get; set; }
        public DateTime Fecha { get; set; }
        public int Minutos { get; set; }

        public Transaccion ToTransaccion()
        {
            return new Transaccion()
            {
                Id = Id.HasValue ? Id.Value : default,
                UserFromId = IdFrom,
                UserToId = IdTo,
                Fecha = Fecha,
                Minutos = Minutos,
                OperacionId = IdOperacion

            };
        }
    }

    public class TransaccionDelegadaDTO
    {
        public TransaccionDelegadaDTO() { }

        public TransaccionDelegadaDTO([NotNull] TransaccionDelegada transaccion)
        {
            IdOperacion = transaccion.OperacionId;
            IdUsuarioADelegar = transaccion.UserId;
            FechaInicio = transaccion.Inicio;
            FechaFin = transaccion.Fin;
        }

        public long IdOperacion { get; set; }
        public long IdUsuarioADelegar { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
