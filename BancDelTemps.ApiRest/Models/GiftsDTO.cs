using System;
using System.Collections.Generic;
using System.Linq;

namespace BancDelTemps.ApiRest.Models
{
    public class GiftsDTO
    {
       
        public GiftsDTO() { }
        public GiftsDTO(User user,Context context)
        {
            IdUser = user.Id;
            Recived = context.GetUserGifts(user)
                        .Where(g=>g.Transaccion.UserToId==user.Id)
                        .Select(g=>new GiftDTO(g));

            Gifted = context.GetUserGifts(user)
                         .Where(g=>g.Transaccion.UserFromId==user.Id)
                         .Select(g=>new GiftDTO(g));
        }
        public long UserId { get; set; }
        public IEnumerable<GiftDTO> Recived { get; set; }
        public IEnumerable<GiftDTO> Gifted { get; set; }

    }
    public class GiftDTO{
        public GiftDTO(Gift gift){
            GiftId=gift.Id;
            Transaccion=new TransaccionDTO(gift.Transaccion);
        }
        public long GiftId{get;set;}
        public TransaccionDTO Transaccion{get;set;}
    }
}