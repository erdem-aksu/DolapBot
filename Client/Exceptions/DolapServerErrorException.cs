using System;

namespace DolapBot.Client.Exceptions
{
    public class DolapServerErrorException : DolapException
    {
        public DolapServerErrorException()
        {
        }

        public DolapServerErrorException(string message)
            : base(message)
        {
        }

        public DolapServerErrorException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}