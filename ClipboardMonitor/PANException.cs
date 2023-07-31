using System;
using System.Collections;

namespace ClipboardMonitor
{
    [Serializable]
    public sealed class PANException : Exception
    {
        public PANException()
        {
        }

        public PANException(string message) : base(message)
        {
        }

        public PANException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public override IDictionary Data => base.Data;

        public override string Message => base.Message;

        private PANException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}
