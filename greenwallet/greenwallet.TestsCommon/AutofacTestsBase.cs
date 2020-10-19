using System;
using System.Globalization;
using System.Reflection;
using Autofac;
using Autofac.Core;

namespace greenwallet.TestsCommon
{
    public abstract class AutofacTestsBase : IDisposable
    {
        private IContainer AutofacContainer { get; }
        protected ILifetimeScope LifeTimeScope { get; }
        //protected IPrepareDataService PrepareDataService { get; }
        //public IUnitOfWork UnitOfWork { get; }

        protected AutofacTestsBase()
        {
            ContainerBuilder builder = ConfigureContainerBuilder();

            builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());

            AutofacContainer = builder.Build();
            LifeTimeScope = AutofacContainer.BeginLifetimeScope();
            //PrepareDataService = Resolve<IPrepareDataService>();
            //UnitOfWork = Resolve<IUnitOfWork>();
            //UnitOfWork.Start();

            CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("sk-SK");
        }

        private ContainerBuilder ConfigureContainerBuilder()
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());

            //builder.RegisterGeneric(typeof(CollectionRepository<>)).As(typeof(IRepository<>)).InstancePerLifetimeScope();
            //builder.RegisterType<SimpleUnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();

            //builder.Register(c => new Mock<ILogService>().Object).As<ILogService>().SingleInstance();

            ConfigureContainerBuilder(builder);

            return builder;
        }

        protected virtual void ConfigureContainerBuilder(ContainerBuilder builder)
        {
        }

        protected T Resolve<T>()
        {
            return LifeTimeScope.Resolve<T>();
        }

        protected TEntity Resolve<TEntity>(Parameter parameter)
        {
            return LifeTimeScope.Resolve<TEntity>(parameter);
        }
        protected TEntity Resolve<TEntity>(Parameter[] parameters)
        {
            return LifeTimeScope.Resolve<TEntity>(parameters);
        }

        public void Dispose()
        {
            LifeTimeScope?.Dispose();
            AutofacContainer?.Dispose();
        }
    }
}