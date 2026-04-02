using ETQAN_BY_API.DTO;

namespace ETQAN_BY_API.Services
{
    public interface IEmailServices
    {
        public void SendEmail(EmailDTO request);
    }
}
