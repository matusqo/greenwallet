using System;
using System.Threading.Tasks;
using greenwallet.Model;
using greenwallet.Persistence.Interfaces;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Wrap;

namespace greenwallet.Logic
{
    public class WalletMovementsHandler : IWalletMovementsHandler
    {
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletHandler _walletHandler;
        private readonly AsyncPolicyWrap _concurrencyConflictsResilientPolicy;

        public WalletMovementsHandler(IWalletTransactionRepository walletTransactionRepository, IWalletRepository walletRepository, IWalletHandler walletHandler)
        {
            _walletTransactionRepository = walletTransactionRepository;
            _walletRepository = walletRepository;
            _walletHandler = walletHandler;

            PolicyBuilder concurrencyExceptionHandler = Policy.Handle<ConcurrentWriteConflictException>();
            AsyncRetryPolicy waitAndRetryPolicy = concurrencyExceptionHandler.WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(2 * i));
            AsyncRetryPolicy retryPolicy = concurrencyExceptionHandler.RetryAsync(3);
            _concurrencyConflictsResilientPolicy = waitAndRetryPolicy.WrapAsync(retryPolicy);
        }

        public async Task<TransactionStatus> DepositFunds(WalletMovementRequest walletMovementRequest)
        {
            ValidateMovementRequest(walletMovementRequest);

            Wallet wallet = await _walletHandler.GetWallet(walletMovementRequest.WalletExternalId).ConfigureAwait(false);

            return await _concurrencyConflictsResilientPolicy.ExecuteAsync(async () =>
            {
                WalletTransaction walletTransaction = await _walletTransactionRepository.Get(walletMovementRequest.MovementExternalId).ConfigureAwait(false);
                if (walletTransaction != null)
                    return walletTransaction.Status;

                decimal walletBalance = await _walletHandler.GetWalletBalance(wallet.Id).ConfigureAwait(false);
                await _walletTransactionRepository.Add(new WalletTransaction
                {
                    ExternalId = walletMovementRequest.MovementExternalId,
                    Type = TransactionType.Deposit,
                    Status = TransactionStatus.Accepted,
                    Amount = walletMovementRequest.Amount,
                    Balance = walletBalance + walletMovementRequest.Amount,
                    Wallet = wallet
                }).ConfigureAwait(false);

                return TransactionStatus.Accepted;
            });
        }

        public async Task<TransactionStatus> WithdrawFunds(WalletMovementRequest walletMovementRequest)
        {
            ValidateMovementRequest(walletMovementRequest);
            
            Wallet wallet = await _walletHandler.GetWallet(walletMovementRequest.WalletExternalId).ConfigureAwait(false);

            return await _concurrencyConflictsResilientPolicy.ExecuteAsync(async () => 
            {
                WalletTransaction walletTransaction = await _walletTransactionRepository.Get(walletMovementRequest.MovementExternalId).ConfigureAwait(false);
                if (walletTransaction != null)
                    return walletTransaction.Status;

                decimal walletBalance = await _walletHandler.GetWalletBalance(wallet.Id).ConfigureAwait(false);
                if (walletBalance - walletMovementRequest.Amount < 0m)
                {
                    await _walletTransactionRepository.Add(new WalletTransaction
                    {
                        ExternalId = walletMovementRequest.MovementExternalId,
                        Type = TransactionType.Stake,
                        Status = TransactionStatus.Rejected,
                        Amount = walletMovementRequest.Amount,
                        Balance = walletBalance,
                        Wallet = wallet
                    }).ConfigureAwait(false);
                    return TransactionStatus.Rejected;
                }

                await _walletTransactionRepository.Add(new WalletTransaction
                {
                    ExternalId = walletMovementRequest.MovementExternalId,
                    Type = TransactionType.Stake,
                    Status = TransactionStatus.Accepted,
                    Amount = walletMovementRequest.Amount,
                    Balance = walletBalance - walletMovementRequest.Amount,
                    Wallet = wallet
                }).ConfigureAwait(false);

                return TransactionStatus.Accepted;
            });
        }

        private static void ValidateMovementRequest(WalletMovementRequest walletMovementRequest)
        {
            if (string.IsNullOrEmpty(walletMovementRequest.WalletExternalId))
                throw new ArgumentException("email is missing", nameof(walletMovementRequest.WalletExternalId));
            if (string.IsNullOrEmpty(walletMovementRequest.MovementExternalId))
                throw new ArgumentException("missing external ID", nameof(walletMovementRequest.MovementExternalId));
            if (walletMovementRequest.Amount <= 0m)
                throw new ArgumentException("amount is less than zero", nameof(walletMovementRequest.Amount));
        }

        private async Task<Wallet> GetWallet(string externalId)
        {
            Wallet wallet = await _walletRepository.Get(externalId).ConfigureAwait(false);
            if (wallet == null)
                throw new Exception("wallet does not exist");
            return wallet;
        }
    }
}