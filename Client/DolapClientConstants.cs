namespace DolapBot.Client
{
    public class DolapClientConstants
    {
        public const string UserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.150 Safari/537.36";

        public const string BaseUrl = "https://dolap.com";
        public const string LoginUrl = "/giris";
        public const string RegisterUrl = "/kayit";
        
        public const string FollowUrl = "/member/follow/{id}";
        public const string UnFollowUrl = "/member/unfollow/{id}";

        public const string LikeUrl = "/product/like/{id}";
        public const string UnLikeUrl = "/product/unlike/{id}";
        
        public const string CommentUrl = "/product/comment";

        public const string CaptchaApiRequestUrl = "https://2captcha.com/in.php";
        public const string CaptchaApiResponseUrl = "https://2captcha.com/res.php";
        public const string CaptchaApiKey = "YOUR_API_KEY";
    }
}