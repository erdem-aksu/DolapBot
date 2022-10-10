using System;

namespace DolapBot.Client.Exceptions
{
    public class DolapRateLimitExceededException : DolapException
    {
        public DolapRateLimitExceededException()
        {
            HttpStatusCode = 429;
        }

        public DolapRateLimitExceededException(string message)
            : base(message)
        {
            HttpStatusCode = 429;
        }

        public DolapRateLimitExceededException(string message, Exception inner)
            : base(message, inner)
        {
            HttpStatusCode = 429;
        }
    }
}