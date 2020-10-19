//using System;

//namespace greenwallet.TestsCommon
//{
//    public abstract class TestsBase : IDisposable
//    {
//        public IServiceScope ServiceScope { get; }
//        public ServiceProvider ServiceProvider { get; }

//        protected TestsBase()
//        {
//            ServiceProvider = ConfigureServiceCollection().BuildServiceProvider();
//            ServiceScope = ServiceProvider.CreateScope();
//        }

//        private ServiceCollection ConfigureServiceCollection()
//        {
//            var serviceCollection = new ServiceCollection();
//            serviceCollection.AddTransient<IWalletMovementsHandler, WalletMovementsHandler>();

//            ConfigureServiceCollection(serviceCollection);

//            return serviceCollection;
//        }

//        protected virtual void ConfigureServiceCollection(ServiceCollection serviceCollection)
//        {
//        }

//        protected T Resolve<T>()
//        {
//            return ServiceScope.ServiceProvider.GetService<T>();
//        }

//        public void Dispose()
//        {
//            ServiceScope?.Dispose();
//            ServiceProvider?.Dispose();
//        }
//    }
//}