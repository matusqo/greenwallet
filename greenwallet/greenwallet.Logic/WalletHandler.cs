using System;
using System.Threading.Tasks;
using greenwallet.Model;
using greenwallet.Persistence.Interfaces;

namespace greenwallet.Logic
{
    public class WalletHandler : IWalletHandler
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;

        public WalletHandler(IWalletRepository walletRepository, IWalletTransactionRepository walletTransactionRepository)
        {
            _walletRepository = walletRepository;
            _walletTransactionRepository = walletTransactionRepository;
        }

        public async Task RegisterNew(string externalId)
        {
            if(string.IsNullOrEmpty(externalId))
                throw new ArgumentException("external ID wasn't provided", nameof(externalId));

            if(await _walletRepository.Get(externalId).ConfigureAwait(false) != null)
                throw new Exception("wallet already exists");

            await _walletRepository.Add(new Wallet
            {
                ExternalId = externalId
            });
        }

        public async Task<decimal> GetWalletBalance(string externalId)
        {
            Wallet wallet = await GetWallet(externalId).ConfigureAwait(false);
            return await GetWalletBalance(wallet.Id).ConfigureAwait(false);
        }

        public async Task<decimal> GetWalletBalance(Guid walletId)
        {
            WalletTransaction lastTransaction = await _walletTransactionRepository.GetLast(walletId).ConfigureAwait(false);
            return lastTransaction?.Balance ?? 0m;
        }

        public async Task<Wallet> GetWallet(string externalId)
        {
            Wallet wallet = await _walletRepository.Get(externalId).ConfigureAwait(false);
            return wallet ?? throw new Exception("wallet does not exist");
        }
    }
}