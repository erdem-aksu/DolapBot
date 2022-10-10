namespace DolapBot.Client.Exceptions
{
    public class DolapBadRequestException : DolapException
    {
        public DolapBadRequestException(string message)
            : base(message)
        {
            HttpStatusCode = 400;
        }
    }
}