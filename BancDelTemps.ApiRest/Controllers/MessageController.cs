using BancDelTemps.ApiRest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancDelTemps.ApiRest.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json", new string[] { "text/plain", "text/json" })]
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MessageDTO[]))]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public IActionResult GetAll()
        {
            return GetAll(0);
        }
        //get fromTicks
        [HttpGet("ticksUTCLastTime:long")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(MessageDTO[]))]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
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
            else result = this.NotLoggedIn();

            return result;

        }
        //hide
        [HttpPost("Hide/{idMessage:long}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> HideMessage(long idMessage)
        {
            IActionResult result;
            User user;
            Message message;

            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                message = await Context.Messages.Where(m => Equals(m.Id, idMessage)).FirstOrDefaultAsync();
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
            else result = this.NotLoggedIn();

            return result;
        }
        //hideAll
        [HttpPost("All/Hide/{idMessageLast:long}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(int))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> HideAllMessage(long idMessageLast)
        {
            IActionResult result;
            User user;
            Message lastMessage;
            int totalHidden = 0;
            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                lastMessage = await Context.Messages.Where(m => Equals(m.Id, idMessageLast)).FirstOrDefaultAsync();
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
            else result = this.NotLoggedIn();

            return result;
        }
        //readed
        [HttpPost("Readed/{idMessage:long}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> ReadedMessage(long idMessage)
        {
            IActionResult result;
            User user;
            Message message;

            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                message = await Context.Messages.Where(m => Equals(m.Id, idMessage)).FirstOrDefaultAsync();
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
            else result = this.NotLoggedIn();

            return result;
        }
        //mark to revise
        [HttpPost("MarkToRevise/{idMessage:long}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> MarkToRevise(long idMessage)
        {
            IActionResult result;
            User user;
            Message message;

            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                message =await Context.Messages.Where(m => Equals(m.Id, idMessage)).FirstOrDefaultAsync();
                if (!Equals(message, default))
                {
                    if (Equals(user.Id, message.ToId))
                    {

                        if (!message.DateMarkedToRevision.HasValue)
                        {
                            message.DateMarkedToRevision = System.DateTime.UtcNow;
                            message.LastUpdateDate = System.DateTime.UtcNow;
                            await Context.SaveChangesAsync();

                        }
                        result = Ok();

                    }
                    else result = Unauthorized();
                }
                else result = NotFound();
            }
            else result = this.NotLoggedIn();

            return result;
        }
        //isRevised
        [HttpGet("MarkToRevise/{idMessage:long}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(bool))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> IsRevised(long idMessage)
        {
            IActionResult result;
            User user;
            Message message;

            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                message = await Context.Messages.Where(m => Equals(m.Id, idMessage)).FirstOrDefaultAsync();
                if (!Equals(message, default))
                {
                    if (Equals(user.Id, message.ToId) || user.IsModMessages)
                    {

                        result = Ok(message.DateRevised.HasValue);

                    }
                    else result = Unauthorized();
                }
                else result = NotFound();
            }
            else result = this.NotLoggedIn();

            return result;
        }
        //toRevisar
        [HttpGet("ToRevise")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MessageDTO[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> ToRevise()
        {
            return await ToRevise(0);
        }
        [HttpGet("ToRevise/{ticksUTCLast:long}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(MessageDTO[]))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> ToRevise(long ticksUTCLast)
        {
            IActionResult result;
            User user;
            IEnumerable<Message> messages;

            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (user.IsModMessages)
                {
                    messages = Context.Messages.Where(m => m.DateMarkedToRevision.HasValue && !m.DateRevised.HasValue && m.Date.Ticks>ticksUTCLast).OrderBy(m=>m.Date);
                   
                    result = Ok(messages.Select(m=>new MessageDTO(m)));

                 
                }
                else result = Unauthorized();
            }
            else result = this.NotLoggedIn();

            return result;
        }
        //revisado
        [HttpPost("ToRevise/{idMessage:long}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> Revisado(long idMessage)
        {
            IActionResult result;
            User user;
            Message message;

            if (ContextoHttp.IsAuthenticated)
            {
                user = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                if (user.IsModMessages)
                {
                    message =await Context.Messages.Where(m => Equals(m.Id, idMessage)).FirstOrDefaultAsync();
                    if (!Equals(message, default))
                    {
                      

                            if (!message.DateRevised.HasValue)
                            {
                                message.DateRevised = System.DateTime.UtcNow;
                                message.LastUpdateDate = System.DateTime.UtcNow;
                                message.Revisor = user;
                                await Context.SaveChangesAsync();

                            }
                            result = Ok();

                     
                    }
                    else result = NotFound();
                }
                else result = Unauthorized();
            }
            else result = this.NotLoggedIn();

            return result;
        }
        //send
        [HttpPost("Send")]
        [Authorize]
        [ProducesResponseType(OwnStatusCodes.ContentForbbiden)]
        [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(MessageDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
        public async Task<IActionResult> Send(MessageDTO messageDTO)
        {
            IActionResult result;
            User userFrom,userTo;
            Message message;

            if (ContextoHttp.IsAuthenticated)
            {
                if (!Equals(messageDTO, default))
                {
                    userFrom = Context.GetUserPermiso(Models.User.GetEmailFromHttpContext(ContextoHttp));
                    userTo = Context.GetUserPermiso(messageDTO.ToId);
                    //si no esta validado y ha esperado lo suficiente entonces puede reclamar ser validado sino nada
                    if (!Equals(userTo, default) && (userFrom.IsValidated || (userTo.IsModValidation && userFrom.CanSendMessageToValidators)))
                    {
                        if (AutoValidateContentText(messageDTO.Text))
                        {
                            message = new Message();
                            message.FromId = userFrom.Id;
                            message.ToId = userTo.Id;
                            message.Text = messageDTO.Text;
                            message.Date = System.DateTime.UtcNow;
                            Context.Messages.Add(message);
                            await Context.SaveChangesAsync();
                            result = Ok(new MessageDTO(message));
                        }
                        else
                        {
                            //el contenido no es apto para la red
                            result = this.ContentForbbiden();
                        }
                    }
                    else result = Unauthorized();
                }
                else result = BadRequest();
            }
            else result = this.NotLoggedIn();

            return result;
        }

        private bool AutoValidateContentText(string text)
        {//en el futuro usar IA para determinar si el contenido es apto o no para la red
            return true;
        }

        //delete all
        [HttpDelete("Clear")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK,Type=typeof(int))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerResponse(OwnStatusCodes.NotLoggedIn, OwnMessage.NotLoggedIn, typeof(ContentResult))]
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
            else result = this.NotLoggedIn();

            return result;
        }
    }
}
