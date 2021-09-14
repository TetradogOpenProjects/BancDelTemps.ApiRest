using System;
using System.Collections.Generic;
using System.Linq;

namespace BancDelTemps.ApiRest.Models
{
    public class GiftsDTO
    {
       
        public GiftsDTO() { }
        public GiftsDTO(User user, Context context) : this(user.Id, context) { }
       
        public GiftsDTO(long userId,Context context)
        {
            UserId = userId;
            Recived = context.GetUserGifts(userId)
                        .Where(g=>g.Transaccion.UserToId== userId)
                        .Select(g=>new GiftDTO(g));

            Gifted = context.GetUserGifts(userId)
                         .Where(g=>g.Transaccion.UserFromId== userId)
                         .Select(g=>new GiftDTO(g));
        }
        public long UserId { get; set; }
        public IEnumerable<GiftDTO> Recived { get; set; }
        public IEnumerable<GiftDTO> Gifted { get; set; }

    }
    public class GiftDTO{

        public GiftDTO() { }
        public GiftDTO(Gift gift){
            GiftId=gift.Id;
            TransaccionId=gift.TransaccionId;
        }
        public long GiftId{get;set;}
        public long TransaccionId {get;set;}
    }
}