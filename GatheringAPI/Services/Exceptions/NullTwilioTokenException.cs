using System;
using System.Runtime.Serialization;

namespace GatheringAPI.Services
{
    [Serializable]
    internal class NullTwilioTokenException : Exception
    {
        public NullTwilioTokenException() : base("Twilio token is invalid/empty.")
        {
        }
    }
}