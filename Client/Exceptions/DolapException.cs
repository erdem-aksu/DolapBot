using System;

namespace DolapBot.Client.Exceptions
{
    public class DolapException : Exception
    {
        public int? HttpStatusCode { get; internal set; }

        public string RequestUrl { get; internal set; }

        public string ResponseContent { get; internal set; }

        public DolapException()
        {
        }

        public DolapException(string message)
            : base(message)
        {
        }

        public DolapException(string message, Exception inner)
            : base(message, inner)
        {
        }

        internal static T Create<T>(
            string message,
            string requestUrl,
            string responseContent,
            int? httpStatusCode = null)
            where T : DolapException
        {
            var exception = (T) Activator.CreateInstance(typeof(T), message);
            exception.RequestUrl = requestUrl;
            exception.ResponseContent = responseContent;

            if (httpStatusCode != null)
                exception.HttpStatusCode = httpStatusCode;

            return exception;
        }
    }
}