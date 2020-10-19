using System.Threading.Tasks;
using greenwallet.Model;

namespace greenwallet.Logic
{
    public class WalletHandler : IWalletHandler
    {
        public Task RegisterNew(Wallet wallet)
        {
            return null;
        }

        public Task<decimal> GetWalletBalance(string playerEmail)
        {
            return null;
        }
    }
}