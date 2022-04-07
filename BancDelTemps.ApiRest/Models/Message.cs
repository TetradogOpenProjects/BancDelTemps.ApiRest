using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BancDelTemps.ApiRest.Models
{
    public class Message
    {
        public static TimeSpan TimeToLiveHiddenMessages = TimeSpan.FromDays(15);
        public static bool CanDeleteRevised = false;
        public long Id { get; set; }
        public long FromId { get; set; }
        public User From { get; set; }

        public long ToId { get; set; }
        public User To { get; set; }

        public string Text { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public bool IsHiddenFrom { get; set; }
        public bool IsHiddenTo { get; set; }
        public DateTime? DateToAndFromHidden { get; set; }
        public DateTime? DateReaded{ get; set; }
        [NotMapped]
        public bool CanFromHideTo => !DateReaded.HasValue;
        public DateTime? DateMarkedToRevision { get; set; }
        public DateTime? DateRevised { get; set; }

        public DateTime? LastUpdateDate { get; set; }
        public DateTime LastUpdate => !LastUpdateDate.HasValue ? Date : LastUpdateDate.Value;

        public long? RevisorId { get; set; }
        public User Revisor { get; set; }
      

        [NotMapped]/*                                             por si lo hacen desde la BD */
        public bool CanDelete => (DateToAndFromHidden.HasValue || (IsHiddenFrom && IsHiddenTo)) && (!DateMarkedToRevision.HasValue || DateRevised.HasValue && CanDeleteRevised);


        public bool IsFromOrToAndCanRead(long id)
        {
            return (id == FromId && !IsHiddenFrom || id == ToId && !IsHiddenTo);
        }
    }
    public class MessageDTO
    {
        public MessageDTO(Message message)
        {
            Id = message.Id;
            FromId = message.FromId;
            ToId = message.ToId;
            Text = message.Text;
            Date = message.Date;
            DateReaded = message.DateReaded;
            DateMarkedToRevision = message.DateMarkedToRevision;
            DateRevised = message.DateRevised;
            RevisorId = message.RevisorId;
        }
        public long Id { get; set; }
        public long FromId { get; set; }

        public long ToId { get; set; }

        public string Text { get; set; }
        public DateTime Date { get; set; }

        public DateTime? DateReaded { get; set; }
        public DateTime? DateMarkedToRevision { get; set; }
        public DateTime? DateRevised { get; set; }
        public long? RevisorId { get; set; }

    }
}
