namespace ETQAN_BY_API.Services
{
    public class OtpGenerator
    {
        public static string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(1000, 9999).ToString();
        }
    }
}
