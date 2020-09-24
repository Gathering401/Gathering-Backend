using System;
using System.Runtime.Serialization;

namespace GatheringAPI.Services
{
    [Serializable]
    internal class NullTwilioSidException : Exception
    {
        public NullTwilioSidException() : base("Twilio sid is invalid/empty.")
        {
        }
    }
}