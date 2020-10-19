using System;
using System.Threading.Tasks;
using greenwallet.Model;
using greenwallet.Persistence.Interfaces;

namespace greenwallet.Logic
{
    public class WalletMovementsHandler : IWalletMovementsHandler
    {
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IWalletRepository _walletRepository;

        public WalletMovementsHandler(IWalletTransactionRepository walletTransactionRepository, IWalletRepository walletRepository)
        {
            _walletTransactionRepository = walletTransactionRepository;
            _walletRepository = walletRepository;
        }

        public async Task<TransactionStatus> DepositFunds(WalletMovementRequest walletMovementRequest)
        {
            if(string.IsNullOrEmpty(walletMovementRequest.PlayerEmail))
                throw new ArgumentException("email is missing", nameof(walletMovementRequest.PlayerEmail));
            if(string.IsNullOrEmpty(walletMovementRequest.ExternalId))
                throw new ArgumentException("missing external ID", nameof(walletMovementRequest.ExternalId));
            if(walletMovementRequest.Amount <= 0m)
                throw new ArgumentException("amount is less than zero", nameof(walletMovementRequest.Amount));

            Wallet wallet = await _walletRepository.Get(walletMovementRequest.PlayerEmail).ConfigureAwait(false);
            if(wallet == null)
                throw new Exception("wallet does not exist");

            WalletTransaction walletTransaction = await _walletTransactionRepository.Get(walletMovementRequest.ExternalId).ConfigureAwait(false);
            if (walletTransaction != null)
                return walletTransaction.Status;

            await _walletTransactionRepository.Add(new WalletTransaction
            {
                ExternalId = walletMovementRequest.ExternalId,
                Type = TransactionType.Deposit,
                Status = TransactionStatus.Accepted,
                Amount = walletMovementRequest.Amount,
                Wallet = wallet
            }).ConfigureAwait(false);

            return TransactionStatus.Accepted;
        }

        public Task<TransactionStatus> WithdrawFunds(WalletMovementRequest walletMovementRequest)
        {
            throw new NotImplementedException();
        }
    }
}