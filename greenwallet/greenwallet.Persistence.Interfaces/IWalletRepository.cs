using System;
using System.Threading.Tasks;
using greenwallet.Model;

namespace greenwallet.Persistence.Interfaces
{
    public interface IWalletRepository
    {
        Task Add(Wallet wallet);
        Task<Wallet> Get(Guid id);
        Task<Wallet> Get(string externalId);
    }
}
