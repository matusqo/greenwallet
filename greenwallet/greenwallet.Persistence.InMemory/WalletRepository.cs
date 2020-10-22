using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using greenwallet.Model;
using greenwallet.Persistence.Interfaces;

namespace greenwallet.Persistence.InMemory
{
    internal class WalletRepository : IWalletRepository
    {
        private readonly Dictionary<Guid, Wallet> _wallets = new Dictionary<Guid, Wallet>();
        private readonly Dictionary<string, Guid> _externalIdMap = new Dictionary<string, Guid>();

        public Task Add(Wallet wallet)
        {
            if(_externalIdMap.ContainsKey(wallet.ExternalId))
                throw new Exception("wallet already exists");

            wallet.Id = Guid.NewGuid();
            _wallets[wallet.Id] = wallet;

            _externalIdMap.Add(wallet.ExternalId, wallet.Id);

            return Task.CompletedTask;
        }

        public Task<Wallet> Get(Guid id)
        {
            _wallets.TryGetValue(id, out Wallet wallet);
            return Task.FromResult(wallet);
        }

        public Task<Wallet> Get(string externalId)
        {
            if (!_externalIdMap.TryGetValue(externalId, out Guid walletId))
                return Task.FromResult<Wallet>(null);

            return Task.FromResult(_wallets[walletId]);
        }
    }
}