using System;
using System.Threading.Tasks;
using greenwallet.Model;

namespace greenwallet.Logic
{
    public interface IWalletHandler
    {
        Task RegisterNew(string externalId);
        Task<decimal> GetWalletBalance(string externalId);
        Task<decimal> GetWalletBalance(Guid walletId);

        /// <summary>
        /// Get wallet by its external ID
        /// </summary>
        /// <param name="externalId">external ID, e.g. e-mail address</param>
        /// <returns>wallet found by external ID</returns>
        /// <exception cref="Exception">if wallet does not exist</exception>
        Task<Wallet> GetWallet(string externalId);
    }
}
