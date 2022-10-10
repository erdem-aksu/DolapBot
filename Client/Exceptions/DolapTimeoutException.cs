using System;

namespace DolapBot.Client.Exceptions
{
    public class DolapTimeoutException : DolapException
    {
        public DolapTimeoutException()
        {
            HttpStatusCode = 408;
        }

        public DolapTimeoutException(string message)
            : base(message)
        {
            HttpStatusCode = 408;
        }

        public DolapTimeoutException(string message, Exception inner)
            : base(message, inner)
        {
            HttpStatusCode = 408;
        }
    }
}