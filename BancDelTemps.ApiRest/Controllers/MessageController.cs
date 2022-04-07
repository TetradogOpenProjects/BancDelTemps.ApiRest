﻿using BancDelTemps.ApiRest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancDelTemps.ApiRest.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController : Controller
    {
        Context Context { get; set; }
        public IHttpContext ContextoHttp { get; set; }
        public MessageController(Context context)
        {
            Context = context;
            ContextoHttp = new ContextoHttp(HttpContext);
        }

        //get all
        [HttpGet]
        [Authorize]
        public IActionResult GetAll()
        {
            return GetAll(0);
        }
        //get fromTicks
        [HttpGet("ticksUTCLastTime:long")]
        [Authorize]
        public IActionResult GetAll(long ticksUTCLastTime)
        {
            IActionResult result;
            User user;

            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                result = Ok(Context.Messages.Where(m => m.IsFromOrToAndCanRead(user.Id) && m.LastUpdate.Ticks > ticksUTCLastTime)
                                            .OrderBy(m => m.Date)
                                            .Select(m => new MessageDTO(m))
                           );
            }
            else result = Forbid();

            return result;

        }
        //hide
        [HttpPost("hide/{idMessage:long}")]
        [Authorize]
        public async Task<IActionResult> HideMessage(long idMessage)
        {
            IActionResult result;
            User user;
            Message message;

            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                message = Context.Messages.Where(m => Equals(m.Id, idMessage)).FirstOrDefault();
                if (!Equals(message, default))
                {
                    if (!message.IsHiddenFrom || !message.IsHiddenTo)
                    {

                        if (user.Id == message.FromId)
                        {
                            message.IsHiddenFrom = true;

                            if (message.CanFromHideTo)
                            {
                                message.IsHiddenTo = true;
                            }
                            if (message.IsHiddenTo)
                            {
                                message.DateToAndFromHidden = System.DateTime.UtcNow;
                            }
                            message.LastUpdateDate = System.DateTime.UtcNow;
                            await Context.SaveChangesAsync();
                            result = Ok();
                        }
                        else if (user.Id == message.ToId)
                        {
                            message.IsHiddenTo = true;
                            if (message.IsHiddenFrom)
                            {
                                message.DateToAndFromHidden = System.DateTime.UtcNow;
                            }
                            message.LastUpdateDate = System.DateTime.UtcNow;
                            await Context.SaveChangesAsync();
                            result = Ok();
                        }
                        else if (user.IsModMessages)
                        {
                            message.IsHiddenFrom = true;
                            message.IsHiddenTo = true;
                            message.DateToAndFromHidden = System.DateTime.UtcNow;
                            message.LastUpdateDate = System.DateTime.UtcNow;
                            message.Revisor = user;
                            await Context.SaveChangesAsync();
                            result = Ok();
                        }
                        else result = Unauthorized();
                    }
                    else result = Ok();
                }
                else result = NotFound();
            }
            else result = Forbid();

            return result;
        }
        //hideAll
        [HttpPost("all/hide/{idMessageLast:long}")]
        [Authorize]
        public async Task<IActionResult> HideAllMessage(long idMessageLast)
        {
            IActionResult result;
            User user;
            Message lastMessage;
            int totalHidden = 0;
            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                lastMessage = Context.Messages.Where(m => Equals(m.Id, idMessageLast)).FirstOrDefault();
                if (!Equals(lastMessage, default))
                {

                    //recupero todos los mensajes del chat y los que sean anteriores incluido el ultimo los oculto
                    foreach (Message message in Context.Messages.Where(m => (m.FromId == user.Id && m.ToId == lastMessage.ToId) || (m.ToId == user.Id && m.FromId == lastMessage.FromId) && m.Date.Ticks <= lastMessage.Date.Ticks))
                    {
                        if (!message.IsHiddenFrom || !message.IsHiddenTo)
                        {
                            if (user.Id == message.FromId)
                            {
                                message.IsHiddenFrom = true;
                                if (message.IsHiddenTo)
                                {
                                    message.DateToAndFromHidden = System.DateTime.UtcNow;
                                }
                                message.LastUpdateDate = System.DateTime.UtcNow;
                            }
                            else if (user.Id == message.ToId)
                            {
                                message.IsHiddenTo = true;
                                if (message.IsHiddenFrom)
                                {
                                    message.DateToAndFromHidden = System.DateTime.UtcNow;
                                }
                                message.LastUpdateDate = System.DateTime.UtcNow;
                            }
                            totalHidden++;
                        }
                    }
                    if (totalHidden > 0)
                    {
                        await Context.SaveChangesAsync();
                    }
                    result = Ok(totalHidden);


                }
                else result = NotFound();
            }
            else result = Forbid();

            return result;
        }
        //readed
        [HttpPost("readed/{idMessage:long}")]
        [Authorize]
        public async Task<IActionResult> ReadedMessage(long idMessage)
        {
            IActionResult result;
            User user;
            Message message;

            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                message = Context.Messages.Where(m => Equals(m.Id, idMessage)).FirstOrDefault();
                if (!Equals(message, default))
                {
                    if (Equals(user.Id, message.FromId))
                    {

                        if (!message.DateReaded.HasValue)
                        {
                            message.DateReaded = System.DateTime.UtcNow;
                            message.LastUpdateDate = System.DateTime.UtcNow;
                            await Context.SaveChangesAsync();

                        }
                        result = Ok();

                    }
                    else result = Unauthorized();
                }
                else result = NotFound();
            }
            else result = Forbid();

            return result;
        }
        //mark to revise
        //isRevised
        //toRevisar
        //send
        //delete all
       
        [HttpDelete("Clear")]
        [Authorize]
        public async Task<IActionResult> Clear()
        {
            IActionResult result;
            User user;
            Message[] messagesToDelete;

            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (user.IsAdmin)
                {
                    messagesToDelete = Context.Messages.Where(m => m.CanDelete).ToArray();
                    Context.Messages.RemoveRange(messagesToDelete);
                    await Context.SaveChangesAsync();
                    result = Ok(messagesToDelete.Length);//así sabe cuanto se ha borrado
                }
                else result = Unauthorized();
            }
            else result = Forbid();

            return result;
        }
    }
}