using greenwallet.Persistence.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace greenwallet.Persistence.InMemory
{
    public static class ServiceCollectionExtensions
    {
        public static void AddPersistenceServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IWalletRepository, WalletRepository>();
            serviceCollection.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
        }
    }
}