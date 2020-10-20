namespace greenwallet.Logic
{
    public class WalletMovementRequest
    {
        public string WalletExternalId { get; set; }
        public decimal Amount { get; set; }
        public string MovementExternalId { get; set; }
    }
}