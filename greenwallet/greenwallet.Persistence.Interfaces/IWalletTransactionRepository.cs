using System;
using System.Threading.Tasks;
using greenwallet.Model;

namespace greenwallet.Persistence.Interfaces
{
    public interface IWalletTransactionRepository
    {
        Task Add(WalletTransaction walletTransaction);
        Task<WalletTransaction> Get(Guid walletId, string externalId);
        Task<WalletTransaction> GetLast(Guid walletId);
    }
}