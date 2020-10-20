using System;

namespace greenwallet.Model
{
    public class WalletTransaction
    {
        public Guid Id { get; set; }
        public string ExternalId { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public TransactionStatus Status { get; set; }
        public Wallet Wallet { get; set; }
    }
}