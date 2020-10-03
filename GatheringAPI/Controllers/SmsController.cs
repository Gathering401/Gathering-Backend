using GatheringAPI.Models;
using GatheringAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twilio;
using Twilio.AspNet.Common;
using Twilio.AspNet.Core;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;

namespace GatheringAPI.Controllers
{
    public class SmsController : TwilioController
    {
        public IConfiguration Configuration { get; }
        public readonly IUser repository;
        public readonly IEvent eventRepo;

        public SmsController(IConfiguration configuration, IUser repository, IEvent eventRepo)
        {

            Configuration = configuration;
            this.repository = repository;
            this.eventRepo = eventRepo;
        }

        public string _accountSid = null;
        public string _authToken = null;
        public string _phone = null;

        // POST: Via Text Message to Twilio #

        // Example Response ->
        // If YES -> look for most recently sent message, and confirm as attending.
        // If NO -> look for most recently sent message, and confirm as not attending.
        // If MAYBE -> look for most recently sent message, don't confirm anything.
        // If HELP -> display different RSVP options again.
        // If anything else is sent -> respond and say "Invalid response, please send HELP to see options.

        [HttpPost("sms")]
        public async Task<TwiMLResult> Index(SmsRequest incomingMessage)
        {
            var messagingResponse = new MessagingResponse();

            string RsvpResponse = incomingMessage.Body;
            string cleanPhone = incomingMessage.From.Remove(0, 2);
            var user = await repository.GetUserByPhone(cleanPhone);
            var invite = user.Invites.FindLast(i => i.Status == RSVPStatus.Pending);

            if (invite == null)
            {
                messagingResponse.Message("You currently have no pending invitations");
                return TwiML(messagingResponse);
            }

            if (!Int32.TryParse(RsvpResponse, out int CleanResponse))
            {
                if (RsvpResponse.StartsWith("QUESTION"))
                {
                    var comment = new EventComment
                    {
                        Comment = RsvpResponse.Remove(0, 9),
                        UserId = user.Id,
                        EventId = invite.EventId
                    };

                    await repository.SaveComment(comment);

                    messagingResponse.Message($"You have asked {invite.Event.EventHost} \"{RsvpResponse.Remove(0, 9)}\" for the {invite.Event.EventName} event. We'll let you know as soon as the event is updated.");
                }
                else
                {
                    messagingResponse.Message("Invalid response. 1 for Yes, 2 for No, 3 for Maybe, 4 to get more Details, 5 for event description, or 6 to ask a question");
                }
            }
            else
            {
                switch (CleanResponse)
                {
                    case 1:
                        invite.Status = RSVPStatus.Accepted;
                        await repository.SaveStatus(invite);
                        messagingResponse.Message($"You have now RSVP'd to {invite.Event.EventName} as attending");
                        break;

                    case 2:
                        invite.Status = RSVPStatus.Declined;
                        await repository.SaveStatus(invite);
                        messagingResponse.Message($"You have now RSVP'd to {invite.Event.EventName} as not attending");
                        break;

                    case 3:
                        invite.Status = RSVPStatus.Maybe;
                        await repository.SaveStatus(invite);
                        messagingResponse.Message($"You have now RSVP'd to {invite.Event.EventName} as possibly attending");
                        break;
                    case 4:
                        messagingResponse.Message($"{invite.Event.EventName} will be on {invite.Event.Start}. It's located at {invite.Event.Location}. There will {(invite.Event.Food ? "" : "not")} be food. It {(invite.Event.Cost > 0 ? "will cost " + invite.Event.Cost + " dollars" : " is free")}."
                            + " Would you like to attend? 1 for Yes, 2 for No, 3 for Maybe, 4 to get more Details, 5 for event description, or 6 to ask a question."
                            );
                        break;
                    case 5:
                        messagingResponse.Message($"The event description is: {invite.Event.Description}."
                            + " Would you like to attend? 1 for Yes, 2 for No, 3 for Maybe, 4 to get more Details, 5 for event description, or 6 to ask a question."
                            );
                        break;
                    case 6:
                        messagingResponse.Message("What question would you like to leave for the event host? Please start the message with QUESTION.");
                        return TwiML(messagingResponse);
                    default:
                        messagingResponse.Message("Invalid response. 1 for Yes, 2 for No, 3 for Maybe, 4 to get more Details, 5 for event description, or 6 to ask a question");
                        break;
                }
            }
            return TwiML(messagingResponse);
        }

        public async Task<MessagingResponse> PostComment(EventInvite invite, SmsRequest incomingMessage)
        {
            var messagingResponse = new MessagingResponse();

            string eventComment = incomingMessage.Body;
            string cleanPhone = incomingMessage.From.Remove(0, 2);
            var user = await repository.GetUserByPhone(cleanPhone);

            var comment = new EventComment
            {
                Comment = eventComment,
                UserId = user.Id,
                EventId = invite.EventId
            };

            await repository.SaveComment(comment);

            messagingResponse.Message($"You have asked {invite.Event.EventHost} \"{eventComment}\" for the {invite.Event.EventName} event. We'll let you know as soon as the event is updated.");
            return messagingResponse;
        }

        // Example message ->
        // You have been invited to {eventName}! Please RSVP.
        // To RSVP as attend, respond YES. For not attending, respond NO. For unsure, respond MAYBE.

        // GET: api/sms/
        [HttpGet("api/sms/send/{messageBody}/to/{userNum}")]
        public void SendMessage(string messageBody, string userNum)
        {
            _phone = Configuration["Twilio:phone"];
            _accountSid = Configuration["Twilio:accountSid"];
            _authToken = Configuration["Twilio:authToken"];

            TwilioClient.Init(_accountSid, _authToken);

            var message = MessageResource.Create(
                body: messageBody,
                from: new Twilio.Types.PhoneNumber($"+1{_phone}"),
                to: new Twilio.Types.PhoneNumber($"+1{userNum}")
                );


            Console.WriteLine(message.Sid);
        }
    }
}
