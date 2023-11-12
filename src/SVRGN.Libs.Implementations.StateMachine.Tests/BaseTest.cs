using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SVRGN.Libs.Contracts.Service.Logging;
using SVRGN.Libs.Contracts.Service.Object;
using SVRGN.Libs.Implementations.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SVRGN.Libs.Implementations.StateMachine.Tests
{
    [TestClass]
    public abstract class BaseTest
    {
        #region Properties

        protected IObjectService ObjectService;
        protected ILogService LogService;

        #endregion Properties

        #region Methods

        #region Initialize
        [TestInitialize]
        public virtual void Initialize()
        {
            TestBootStrapper.Register(new ServiceCollection());
            this.ObjectService = DiContainer.Resolve<IObjectService>();
            this.LogService = DiContainer.Resolve<ILogService>();
        }
        #endregion Initialize

        #endregion Methods
    }
}
