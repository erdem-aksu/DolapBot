namespace DolapBot.Client.Exceptions
{
    public class DolapAuthorizationException : DolapException
    {
        public DolapAuthorizationException(string message)
            : base(message)
        {
            HttpStatusCode = 401;
        }
    }
}