namespace DolapBot.Client.Models
{
    public class RegisterInput
    {
        public bool CampaignAgreement { get; set; } = false;
        public bool MembershipAgreement { get; set; } = true;
        public string Email { get; set; }
        public string NickName { get; set; }
        public string Password { get; set; }
    }
}