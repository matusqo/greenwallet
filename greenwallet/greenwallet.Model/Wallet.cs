using System;

namespace greenwallet.Model
{
    public class Wallet
    {
        public Guid Id { get; set; }
        public string PlayerEmail { get; set; }
        public decimal Balance { get; set; }
    }
}
