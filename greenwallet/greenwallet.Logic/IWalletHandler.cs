using System;
using System.Threading.Tasks;
using greenwallet.Model;

namespace greenwallet.Logic
{
    public interface IWalletHandler
    {
        Task RegisterNew(Wallet wallet);
        Task<decimal> GetWalletBalance(string playerEmail);
    }
}
