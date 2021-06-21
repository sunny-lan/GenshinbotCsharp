using System;
using System.Runtime.Serialization;

namespace genshinbot.reactive
{
    public class LockInterruptedException : Exception
    {
        public LockInterruptedException()
        {
        }

        public LockInterruptedException(string message) : base(message)
        {
        }

        public LockInterruptedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected LockInterruptedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}



