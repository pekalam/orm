using SimpleORM.Providers;
using SimpleORM.Providers.InMemory;
using Unity;

namespace SimpleORM
{
    public interface IDatabaseDepedencies
    {
        IStateManager StateManager { get; }
    }

    public class DatabaseDepedencies : IDatabaseDepedencies
    {
        public IStateManager StateManager { get; set; }
    }


    public static class InternalDepedencyProvider
    {
        private static UnityContainer _container;

        /// <summary>
        /// Konfiguracja kontenera IoC.
        /// </summary>
        static InternalDepedencyProvider()
        {
            _container = new UnityContainer();
            _container.RegisterType<IStateManager, StateManager>();
            _container.RegisterType<IDatabaseProvider, InMemoryDatabaseProvider>();
        }

        /// <summary>
        /// Zależności klasy Database
        /// </summary>
        public static IDatabaseDepedencies DatabaseDepedencies
        {
            get
            {
                var dep = new DatabaseDepedencies();
                dep.StateManager = _container.Resolve<IStateManager>();
                return dep;
            }
        }

        public static IDatabaseProvider DefaultDatabaseProvider
        {
            get { return _container.Resolve<IDatabaseProvider>(); }
        }

        
    }
}