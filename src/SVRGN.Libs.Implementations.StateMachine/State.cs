using SVRGN.Libs.Contracts.StateMachine;
using System;
using System.Collections.Generic;
using System.Text;

namespace SVRGN.Libs.Implementations.StateMachine
{
    internal class State : IState
    {
        #region Properties

        public string Name { get; private set; }

        public Action EnterAction { get; set; }

        public Action UpdateAction { get; set; }

        public Action ExitAction { get; set; }

        #endregion Properties

        #region Construction

        public State(string Name)
        {
            this.Name = Name;
            EnterAction = null;
            UpdateAction = null;
            ExitAction = null;
        }
        #endregion Construction

        #region Methods

        #region SetEnterAction
        public void SetEnterAction(Action NewEnterAction)
        {
            EnterAction = NewEnterAction;
        }
        #endregion SetEnterAction

        #region SetUpdateAction
        public void SetUpdateAction(Action NewUpdateAction)
        {
            UpdateAction = NewUpdateAction;
        }
        #endregion SetUpdateAction

        #region SetExitAction
        public void SetExitAction(Action NewExitAction)
        {
            ExitAction = NewExitAction;
        }
        #endregion SetExitAction

        #endregion Methods

        #region Events

        #endregion Events
    }
}
