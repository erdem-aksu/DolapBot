namespace DolapBot.Client
{
    public partial class DolapClient
    {
        protected DolapRestClient DolapRestClient { get; }

        public DolapClient(DolapRestClient dolapRestClient)
        {
            DolapRestClient = dolapRestClient;
        }
    }
}