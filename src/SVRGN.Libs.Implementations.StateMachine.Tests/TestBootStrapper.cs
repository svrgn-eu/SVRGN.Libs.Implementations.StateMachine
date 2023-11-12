using Microsoft.Extensions.DependencyInjection;
using SVRGN.Libs.Contracts.Base;
using SVRGN.Libs.Contracts.Service.Base;
using SVRGN.Libs.Contracts.Service.Logging;
using SVRGN.Libs.Contracts.Service.Object;
using SVRGN.Libs.Contracts.StateMachine;
using SVRGN.Libs.Implementations.DependencyInjection;
using SVRGN.Libs.Implementations.Service.ConsoleLogger;
using SVRGN.Libs.Implementations.Service.Object;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace SVRGN.Libs.Implementations.StateMachine.Tests
{
    public static class TestBootStrapper
    {
        //TODO: seperate BootStrapper from Base project and upload into nuget
        #region Properties

        #endregion Properties

        #region Methods

        #region Register

        public static void Register(IServiceCollection services)
        {
            if (!DiContainer.SetServiceCollection(services))
            {
                services = DiContainer.GetServiceCollection();
            }

            TestBootStrapper.RegisterIndependentServices(services);


            DiContainer.SetServiceProvider(services.BuildServiceProvider());
        }
        #endregion Register

        #region RegisterForWindows

        public static void RegisterForWindows(IServiceCollection services)
        {
            if (!DiContainer.SetServiceCollection(services))
            {
                services = DiContainer.GetServiceCollection();
            }

            TestBootStrapper.RegisterIndependentServices(services);

            DiContainer.SetServiceProvider(services.BuildServiceProvider());
        }
        #endregion RegisterForWindows

        #region RegisterIndependentServices: registers OS-agnostic services
        /// <summary>
        /// registers OS-agnostic services
        /// </summary>
        /// <param name="services"></param>
        private static void RegisterIndependentServices(IServiceCollection services)
        {
            services.AddSingleton<ILogService, DebugLogService>();
            services.AddSingleton<IObjectService, ObjectService>();

            services.AddTransient<IStateMachine, StateMachine>();
        }
        #endregion RegisterIndependentServices

        #endregion Methods
    }
}
