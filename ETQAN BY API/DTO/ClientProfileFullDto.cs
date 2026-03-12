namespace ETQAN_BY_API.DTO
{
    public class ClientProfileFullDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Governorate { get; set; }
        public List<HistoryItemDto> History { get; set; }
        public List<ReviewDto> Reviews { get; set; }
        public double OverallRating { get; set; }
    }

    public class HistoryItemDto
    {
        public int RequestId { get; set; }
        public string ProviderName { get; set; }
        public string JobTitle { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
    }

    public class ReviewDto
    {
        public string ReviewerName { get; set; }
        public string ReviewerJob { get; set; }
        public string Comment { get; set; }
        public decimal Rating { get; set; }
        public DateTime Date { get; set; }
    }

  
}