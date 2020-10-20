using System;
using System.Threading.Tasks;
using greenwallet.Model;
using greenwallet.Persistence.Interfaces;

namespace greenwallet.Persistence.InMemory
{
    class WalletTransactionRepository : IWalletTransactionRepository
    {
        public Task Add(WalletTransaction walletTransaction)
        {
            return Task.CompletedTask;
        }

        public Task<WalletTransaction> Get(string externalId)
        {
            return Task.FromResult<WalletTransaction>(null);
        }

        public Task<WalletTransaction> GetLast(Guid walletId)
        {
            return Task.FromResult<WalletTransaction>(null);
        }
    }
}