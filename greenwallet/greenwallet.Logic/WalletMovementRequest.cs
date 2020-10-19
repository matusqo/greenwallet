namespace greenwallet.Logic
{
    public class WalletMovementRequest
    {
        public string PlayerEmail { get; set; }
        public decimal Amount { get; set; }
        public string ExternalId { get; set; }
    }
}