using System;
using System.Runtime.Serialization;

namespace GatheringAPI.Services
{
    [Serializable]
    internal class NullTwilioPhoneException : Exception
    {
        public NullTwilioPhoneException() : base("Twilio phone is invalid/empty.")
        {
        }
    }
}