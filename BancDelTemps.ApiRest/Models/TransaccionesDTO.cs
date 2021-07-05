using System;
using System.Collections.Generic;
using System.Linq;

namespace BancDelTemps.ApiRest.Models
{
    public class TransaccionesDTO
    {
       
        public TransaccionesDTO() { }
        public TransaccionesDTO(User user)
        {
            IdUser = user.Id;
            In = user.TransaccionesIn.Select(s=>new TransaccionDTO(s));
            Out = user.TransaccionesFrom.Select(s => new TransaccionDTO(s));
            Signed = user.TransaccionesSigned.Select(s => new TransaccionDTO(s.Transaccion));

        }
        public long IdUser { get; set; }
        public IEnumerable<TransaccionDTO> In { get; set; }
        public IEnumerable<TransaccionDTO> Out { get; set; }
        public IEnumerable<TransaccionDTO> Signed { get; set; }
    }

    public class TransaccionDTO
    {

        public TransaccionDTO() { }
        public TransaccionDTO(Transaccion s)
        {
            IdFrom = s.UserFromId;
            IdTo = s.UserToId;
            IdValidator = s.UserValidatorId;
            IdTransaccionDelegada = s.TransaccionDelegadaId;
            Fecha = s.Fecha;
            Minutos = s.Minutos;
            IdOperacion = s.OperacionId;
        }
        public long IdFrom { get; set; }
        public long IdTo { get; set; }
        public long? IdValidator { get; set; }
        public long? IdTransaccionDelegada { get; set; }
        public long IdOperacion { get; set; }
        public DateTime Fecha { get; set; }
        public int Minutos { get; set; }

        public Transaccion ToTransaccion()
        {
            return new Transaccion() { UserFromId = IdFrom, UserToId = IdTo, Fecha = Fecha, Minutos =Minutos,TransaccionDelegadaId=IdTransaccionDelegada,UserValidatorId=IdValidator,OperacionId=IdOperacion };
        }
    }

    public class TransaccionDelegadaDTO
    {
        public TransaccionDelegadaDTO() { }

        public TransaccionDelegadaDTO(TransaccionDelegada transaccion)
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