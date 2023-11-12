using SVRGN.Libs.Contracts.StateMachine;
using System;
using System.Collections.Generic;
using System.Text;

namespace SVRGN.Libs.Implementations.StateMachine
{
    internal class StateTransition : IStateTransition
    {
        #region Properties

        public string Name { get; private set; }

        public string FromState { get; private set; }

        public string ToState { get; private set; }

        public Action TransitionAction { get; private set; }

        public Action TransitionEnterAction { get; private set; }

        public Action TransitionExitAction { get; private set; }

        public TimeSpan Duration { get; private set; } = TimeSpan.FromSeconds(0);

        #endregion Properties

        #region Construction

        public StateTransition(string Name, string FromState, string ToState)
        {
            this.Name = Name;
            this.FromState = FromState;
            this.ToState = ToState;
            TransitionAction = null;
        }
        #endregion Construction

        #region Methods

        #region SetTransitionEnterAction
        public void SetTransitionEnterAction(Action NewAction)
        {
            TransitionEnterAction = NewAction;
        }
        #endregion SetTransitionEnterAction

        #region SetTransitionAction
        public void SetTransitionAction(Action NewAction)
        {
            TransitionAction = NewAction;
        }
        #endregion SetTransitionAction

        #region SetTransitionExitAction
        public void SetTransitionExitAction(Action NewAction)
        {
            TransitionExitAction = NewAction;
        }
        #endregion SetTransitionExitAction

        #region SetDuration
        public void SetDuration(TimeSpan Duration)
        {
            this.Duration = Duration;
        }
        #endregion SetDuration

        #endregion Methods      
    }
}
