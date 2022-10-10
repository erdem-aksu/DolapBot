namespace DolapBot.Client.Exceptions
{
    public class DolapNotFoundException : DolapException
    {
        public DolapNotFoundException(string message)
            : base(message)
        {
            HttpStatusCode = 404;
        }
    }
}