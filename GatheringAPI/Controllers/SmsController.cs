using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Twilio;
using Twilio.AspNet.Common;
using Twilio.AspNet.Core;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;

namespace GatheringAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmsController : TwilioController
    {
        public IConfiguration Configuration { get; }
        public SmsController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public string _accountSid = null;
        public string _authToken = null;

        public TwiMLResult Index(SmsRequest incomingMessage)
        {
            var messagingResponse = new MessagingResponse();
            messagingResponse.Message("The copy cat says: " + incomingMessage.Body);

            return TwiML(messagingResponse);
        }

        // GET: api/Sms
        [HttpGet]
        public void SendMessage()
        {
            _accountSid = Configuration["Twilio:accountSid"];
            _authToken = Configuration["Twilio:authToken"];

            TwilioClient.Init(_accountSid, _authToken);

            var message = MessageResource.Create(
                body: "Would you like to party with pizza?",
                from: new Twilio.Types.PhoneNumber("+17652493639"),
                to: new Twilio.Types.PhoneNumber("+13193613414")
                );


            Console.WriteLine(message.Sid);
        }
    }
}
