using System;
using System.Runtime.Serialization;

namespace MassTransit.Persistence.MongoDb
{
    [Serializable]
    public class MongoDbConcurrencyException : Exception
    {
        public MongoDbConcurrencyException()
        {
        }

        public MongoDbConcurrencyException(string message) 
            : base(message)
        {
        }

        public MongoDbConcurrencyException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public MongoDbConcurrencyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}