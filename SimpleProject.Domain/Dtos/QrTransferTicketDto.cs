namespace SimpleProject.Domain.Dtos
{
    public class QrTransferTicketDto : EntityDto
    {
        public int CollarId { get; set; }
        public int FromOwnerUserId { get; set; }
        public int? FromDealerId { get; set; }
        public int? ToOwnerUserId { get; set; }
        public string Status { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
