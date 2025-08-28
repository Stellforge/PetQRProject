namespace SimpleProject.Domain.Entities
{
    public class QrTransferTicket : Entity
    {
        public int CollarId { get; set; }               
        public int FromOwnerUserId { get; set; }        
        public int? FromDealerId { get; set; }          
        public int? ToOwnerUserId { get; set; }         
        public string Status { get; set; }              
        public string Token { get; set; }               
        public DateTime ExpiresAt { get; set; }         
        public DateTime CreatedAt { get; set; }         

        public Collar Collar { get; set; }
        public AppUser FromOwnerUser { get; set; }
        public Dealer? FromDealer { get; set; }
        public AppUser? ToOwnerUser { get; set; }
    }
}
