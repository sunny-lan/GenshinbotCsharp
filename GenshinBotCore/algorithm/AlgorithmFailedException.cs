using System;
using System.Runtime.Serialization;

namespace genshinbot.algorithm
{
    public class AlgorithmFailedException : Exception
    {
        public AlgorithmFailedException()
        {
        }

        public AlgorithmFailedException(string? message) : base(message)
        {
        }

        public AlgorithmFailedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected AlgorithmFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
