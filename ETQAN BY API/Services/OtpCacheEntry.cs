namespace ETQAN_BY_API.Services
{
    public class OtpCacheEntry
    {
        public RegisterArtisanDto UserData { get; set; }
        public string OtpCode { get; set; }
    }

    public class OtpCacheEntry<T>
    {
        public T UserData { get; set; }
        public string OtpCode { get; set; }
    }
}
