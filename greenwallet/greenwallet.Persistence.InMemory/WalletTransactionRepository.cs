using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using greenwallet.Model;
using greenwallet.Persistence.Interfaces;

namespace greenwallet.Persistence.InMemory
{
    internal class WalletTransactionRepository : IWalletTransactionRepository
    {
        private readonly Dictionary<Guid, Dictionary<Guid, WalletTransaction>> _transactions = new Dictionary<Guid, Dictionary<Guid, WalletTransaction>>();
        private readonly Dictionary<string, Guid> _externalIdMap = new Dictionary<string, Guid>();

        public Task Add(WalletTransaction walletTransaction)
        {
            if(_externalIdMap.ContainsKey(walletTransaction.ExternalId))
                throw new Exception("transaction already exists");

            if(!_transactions.ContainsKey(walletTransaction.Wallet.Id))
                _transactions[walletTransaction.Wallet.Id] = new Dictionary<Guid, WalletTransaction>();

            walletTransaction.Id = Guid.NewGuid();
            walletTransaction.DateTime = DateTime.UtcNow;
            _transactions[walletTransaction.Wallet.Id].Add(walletTransaction.Id, walletTransaction);
            _externalIdMap.Add(walletTransaction.ExternalId, walletTransaction.Id);

            return Task.CompletedTask;
        }

        public Task<WalletTransaction> Get(Guid walletId, string externalId)
        {
            if (!_externalIdMap.TryGetValue(externalId, out Guid transactionId))
                return Task.FromResult<WalletTransaction>(null);

            return Task.FromResult(_transactions[walletId][transactionId]);
        }

        public Task<WalletTransaction> GetLast(Guid walletId)
        {
            if (!_transactions.ContainsKey(walletId))
                return Task.FromResult<WalletTransaction>(null);

            return Task.FromResult(_transactions[walletId].Values.OrderByDescending(transaction => transaction.DateTime).LastOrDefault());
        }
    }
}