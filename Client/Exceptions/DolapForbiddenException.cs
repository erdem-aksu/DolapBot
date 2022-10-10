namespace DolapBot.Client.Exceptions
{
    public class DolapForbiddenException : DolapException
    {
        public DolapForbiddenException(string message)
            : base(message)
        {
            HttpStatusCode = 403;
        }
    }
}