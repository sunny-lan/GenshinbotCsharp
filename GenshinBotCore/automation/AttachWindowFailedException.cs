using System;
using System.Runtime.Serialization;

namespace genshinbot.automation
{
    public class AttachWindowFailedException : Exception
    {
        public AttachWindowFailedException()
        {
        }

        public AttachWindowFailedException(string message) : base(message)
        {
        }

        public AttachWindowFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AttachWindowFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}