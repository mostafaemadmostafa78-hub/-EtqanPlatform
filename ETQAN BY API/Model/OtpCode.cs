namespace ETQAN_BY_API.Model
{
 
        public class OtpCode
        {
            public int Id { get; set; }

            public string UserId { get; set; }

            public string Code { get; set; }

            public DateTime ExpireAt { get; set; }

            public bool IsUsed { get; set; } = false;
        }
    
}
