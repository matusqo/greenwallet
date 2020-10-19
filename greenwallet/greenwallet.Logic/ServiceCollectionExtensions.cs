using Microsoft.Extensions.DependencyInjection;

namespace greenwallet.Logic
{
    public static class ServiceCollectionExtensions
    {
        public static void AddLogicServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IWalletHandler, WalletHandler>();
            serviceCollection.AddScoped<IWalletMovementsHandler, WalletMovementsHandler>();
        }
    }
}