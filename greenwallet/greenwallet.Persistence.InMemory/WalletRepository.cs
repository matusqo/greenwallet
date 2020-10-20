using System;
using System.Threading.Tasks;
using greenwallet.Model;
using greenwallet.Persistence.Interfaces;

namespace greenwallet.Persistence.InMemory
{
    class WalletRepository : IWalletRepository
    {
        public Task Add(Wallet wallet)
        {
            return Task.CompletedTask;
        }

        public Task<Wallet> Get(Guid id)
        {
            return Task.FromResult<Wallet>(null);
        }

        public Task<Wallet> Get(string externalId)
        {
            return Task.FromResult<Wallet>(null);
        }
    }
}