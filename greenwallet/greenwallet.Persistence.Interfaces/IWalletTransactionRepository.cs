using System;
using System.Threading.Tasks;
using greenwallet.Model;

namespace greenwallet.Persistence.Interfaces
{
    public interface IWalletTransactionRepository
    {
        Task Add(WalletTransaction walletTransaction);
        Task<WalletTransaction> Get(string externalId);
    }
}