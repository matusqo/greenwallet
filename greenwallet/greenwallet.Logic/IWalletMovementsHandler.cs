using System.Threading.Tasks;
using greenwallet.Model;

namespace greenwallet.Logic
{
    public interface IWalletMovementsHandler
    {
        Task<TransactionStatus> DepositFunds(WalletMovementRequest walletMovementRequest);
        Task<TransactionStatus> WithdrawFunds(WalletMovementRequest walletMovementRequest);
    }
}