using SVRGN.Libs.Contracts.Service.Logging;
using SVRGN.Libs.Contracts.Service.Object;
using SVRGN.Libs.Contracts.StateMachine;
using SVRGN.Libs.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SVRGN.Libs.Implementations.StateMachine
{
    /// <summary>
    /// a rather simple state machine implementation, based on string states to avoid complex enum definition
    /// </summary>
    /// <remarks>
    /// Losely following the explanation on https://deniskyashif.com/2019/11/20/a-practical-guide-to-state-machines/
    /// </remarks>
    public class StateMachine : IStateMachine
    {
        #region Properties

        public int StateCount { get { return states.Count; } }

        public int TransitionCount { get { return transitions.Count; } }

        private List<IState> states;
        private List<IStateTransition> transitions;

        private string currentStateName;

        private ILogService logService;
        private IObjectService objectService;

        public TimeSpan UpdateInterval { get; private set; } = new TimeSpan(0, 0, 0);  //keep the update interval low to enforce quick reactions!
        public DateTime LastUpdatedAt { get; set; }

        #endregion Properties

        #region Construction

        public StateMachine(ILogService LogService, IObjectService ObjectService)
        {
            logService = LogService;
            objectService = ObjectService;

            states = new List<IState>();
            transitions = new List<IStateTransition>();
        }
        #endregion Construction

        #region Methods

        #region SetUpdateInterval
        public void SetUpdateInterval(TimeSpan NewInterval)
        {
            this.UpdateInterval = NewInterval;
        }
        #endregion SetUpdateInterval

        #region AddState
        public bool AddState(string StateName)
        {
            bool result = false;

            if (states.Any(x => x.Name.Equals(StateName)))
            {
                logService.Warning("StateMachine", "AddState", $"State '{StateName}' is already in the list of states, will not be added.");
            }
            else
            {
                states.Add(objectService.Create<State>(StateName));
                logService.Info("StateMachine", "AddState", $"State '{StateName}' has been added.");
            }

            return result;
        }
        #endregion AddState

        #region StartWith
        public void StartWith(string StateName)
        {
            currentStateName = StateName;
            //call enter action
            IState currentState = GetCurrentState();
            if (currentState != null && currentState.EnterAction != null)
            {
                currentState.EnterAction.Invoke();
                logService.Info("StateMachine", "StartWith", $"Finished invoking Enter Action for State '{currentStateName}'.");
            }
        }
        #endregion StartWith

        #region AddTransition
        public bool AddTransition(string TransitionName, string FromStateName, string ToStateName)
        {
            bool result = false;

            if (transitions.Any(x => x.Name.Equals(TransitionName)))
            {
                logService.Warning("StateMachine", "AddTransition", $"Transition '{TransitionName}' is already in the list of transitions, will not be added.");
            }
            else
            {
                IStateTransition newTransition = objectService.Create<StateTransition>(TransitionName, FromStateName, ToStateName);
                transitions.Add(newTransition);
                logService.Info("StateMachine", "AddTransition", $"Transition '{TransitionName}', from '{FromStateName}' to '{ToStateName}', has been added.");
                result = true;
            }

            return result;
        }
        #endregion AddTransition

        #region Transition
        public bool Transition(string TransitionName)
        {
            bool result = false;

            if (TransitionExists(TransitionName))
            {
                IStateTransition stateTransition = GetTransition(TransitionName);
                if (currentStateName.Equals(stateTransition.FromState))
                {
                    //call exit from the previous state
                    IState lastCurrentState = GetCurrentState();
                    if (lastCurrentState != null && lastCurrentState.ExitAction != null)
                    {
                        lastCurrentState.ExitAction.Invoke();
                        logService.Info("StateMachine", "Transition", $"Finished invoking Exit Action for State '{currentStateName}'.");
                    }

                    //call transition enter action
                    if (stateTransition.TransitionEnterAction != null)
                    {
                        stateTransition.TransitionEnterAction.Invoke();
                        logService.Info("StateMachine", "Transition", $"Finished invoking Enter Action for Transition '{stateTransition.Name}'.");
                    }

                    // set new current state
                    currentStateName = stateTransition.ToState;

                    //call start from the current state
                    IState newCurrentState = GetCurrentState();
                    if (newCurrentState != null && newCurrentState.EnterAction != null)
                    {
                        newCurrentState.EnterAction.Invoke();
                        logService.Info("StateMachine", "Transition", $"Finished invoking Enter Action for State '{currentStateName}'.");
                    }

                    //call transition exit action
                    if (stateTransition.TransitionExitAction != null)
                    {
                        stateTransition.TransitionExitAction.Invoke();
                        logService.Info("StateMachine", "Transition", $"Finished invoking Exit Action for Transition '{stateTransition.Name}'.");
                    }

                    logService.Info("StateMachine", "Transition", $"Successfully changed the state from '{stateTransition.FromState}' to '{stateTransition.ToState}', invoking action now.");
                    if (stateTransition.TransitionAction != null)
                    {
                        stateTransition.TransitionAction.Invoke();
                        logService.Info("StateMachine", "Transition", $"Finished invoking Enter Action for Transition '{stateTransition.Name}'.");
                    }
                    else
                    {
                        logService.Info("StateMachine", "Transition", $"No Action defined (is null), not invoking anything.");
                    }
                    result = true;
                }
                else
                {
                    //wrong entry state
                    logService.Error("StateMachine", "Transition", $"Wrong entry state for Transition '{stateTransition.Name}', should be '{stateTransition.FromState}', but is '{currentStateName}'");
                }
            }
            else
            {
                //transition not found
                logService.Error("StateMachine", "Transition", $"Transition '{TransitionName}' not found");
            }

            return result;
        }
        #endregion Transition

        #region GetCurrentState
        public string GetCurrentStateName()
        {
            return currentStateName;
        }
        #endregion GetCurrentState

        #region GetCurrentState
        private IState GetCurrentState()
        {
            return GetState(currentStateName);
        }
        #endregion GetCurrentState

        #region AddTransitionEnterAction
        public bool AddTransitionEnterAction(string TransitionName, Action ActionToTake)
        {
            bool result = false;
            if (TransitionExists(TransitionName))
            {
                IStateTransition stateTransition = GetTransition(TransitionName);
                stateTransition.SetTransitionEnterAction(ActionToTake);
                logService.Info("StateMachine", "AddTransitionEnterAction", $"Action set successfully for '{TransitionName}'");
                result = true;
            }
            else
            {
                //transition not found
                logService.Error("StateMachine", "AddTransitionEnterAction", $"Transition '{TransitionName}' not found");
            }
            return result;
        }
        #endregion AddTransitionEnterAction

        #region AddTransitionAction
        public bool AddTransitionAction(string TransitionName, Action ActionToTake)
        {
            bool result = false;
            if (TransitionExists(TransitionName))
            {
                IStateTransition stateTransition = GetTransition(TransitionName);
                stateTransition.SetTransitionAction(ActionToTake);
                logService.Info("StateMachine", "AddTransitionAction", $"Action set successfully for '{TransitionName}'");
                result = true;
            }
            else
            {
                //transition not found
                logService.Error("StateMachine", "AddTransitionAction", $"Transition '{TransitionName}' not found");
            }
            return result;
        }
        #endregion AddTransitionAction

        #region AddTransitionExitAction
        public bool AddTransitionExitAction(string TransitionName, Action ActionToTake)
        {
            bool result = false;
            if (TransitionExists(TransitionName))
            {
                IStateTransition stateTransition = GetTransition(TransitionName);
                stateTransition.SetTransitionExitAction(ActionToTake);
                logService.Info("StateMachine", "SetTransitionExitAction", $"Action set successfully for '{TransitionName}'");
                result = true;
            }
            else
            {
                //transition not found
                logService.Error("StateMachine", "SetTransitionExitAction", $"Transition '{TransitionName}' not found");
            }
            return result;
        }
        #endregion AddTransitionExitAction

        #region AddStateEnterAction
        public bool AddStateEnterAction(string StateName, Action ActionToTake)
        {
            bool result = false;
            if (StateExists(StateName))
            {
                IState currentState = GetState(StateName);
                currentState.SetEnterAction(ActionToTake);
                logService.Info("StateMachine", "AddStateEnterAction", $"Enter Action set successfully for '{StateName}'");
                result = true;
            }
            else
            {
                //transition not found
                logService.Error("StateMachine", "AddStateEnterAction", $"State '{StateName}' not found");
            }
            return result;
        }
        #endregion AddStateEnterAction

        #region AddStateUpdateAction
        public bool AddStateUpdateAction(string StateName, Action ActionToTake)
        {
            bool result = false;
            if (StateExists(StateName))
            {
                IState currentState = GetState(StateName);
                currentState.SetUpdateAction(ActionToTake);
                logService.Info("StateMachine", "AddStateUpdateAction", $"Update Action set successfully for '{StateName}'");
                result = true;
            }
            else
            {
                //transition not found
                logService.Error("StateMachine", "AddStateUpdateAction", $"State '{StateName}' not found");
            }
            return result;
        }
        #endregion AddStateUpdateAction

        #region AddStateExitAction
        public bool AddStateExitAction(string StateName, Action ActionToTake)
        {
            bool result = false;
            if (StateExists(StateName))
            {
                IState currentState = GetState(StateName);
                currentState.SetExitAction(ActionToTake);
                logService.Info("StateMachine", "AddStateExitAction", $"Exit Action set successfully for '{StateName}'");
                result = true;
            }
            else
            {
                //transition not found
                logService.Error("StateMachine", "AddStateExitAction", $"State '{StateName}' not found");
            }
            return result;
        }
        #endregion AddStateExitAction

        #region Update
        public void Update()
        {
            if (this.ShouldUpdateNow())
            {
                ImmediateUpdate();
            }
        }
        #endregion Update

        #region ImmediateUpdate
        public void ImmediateUpdate()
        {
            //call update from the current state
            IState currentState = GetCurrentState();
            if (currentState != null && currentState.UpdateAction != null)
            {
                currentState.UpdateAction.Invoke();
            }
        }
        #endregion ImmediateUpdate

        #region TransitionExists
        private bool TransitionExists(string Name)
        {
            bool result = false;

            if (transitions.Any(x => x.Name.Equals(Name)))
            {
                result = true;
            }

            return result;
        }
        #endregion TransitionExists

        #region StateExists
        private bool StateExists(string Name)
        {
            bool result = false;

            if (states.Any(x => x.Name.Equals(Name)))
            {
                result = true;
            }

            return result;
        }
        #endregion StateExists

        #region TransitionExistsByFromState
        private bool TransitionExistsByFromState(string StateName)
        {
            bool result = false;

            if (transitions.Any(x => x.FromState.Equals(StateName)))
            {
                result = true;
            }

            return result;
        }
        #endregion TransitionExistsByFromState

        #region TransitionExistsByToState
        private bool TransitionExistsByToState(string StateName)
        {
            bool result = false;

            if (transitions.Any(x => x.ToState.Equals(StateName)))
            {
                result = true;
            }

            return result;
        }
        #endregion TransitionExistsByToState

        #region GetTransition
        private IStateTransition GetTransition(string Name)
        {
            IStateTransition result = default;

            if (TransitionExists(Name))
            {
                result = transitions.Where(x => x.Name.Equals(Name)).FirstOrDefault();
            }

            return result;
        }
        #endregion GetTransition

        #region GetState
        private IState GetState(string Name)
        {
            IState result = default;

            if (StateExists(Name))
            {
                result = states.Where(x => x.Name.Equals(Name)).FirstOrDefault();
            }

            return result;
        }
        #endregion GetState

        #region GetTransitionByFromState
        private IStateTransition GetTransitionByFromState(string StateName)
        {
            IStateTransition result = default;

            if (TransitionExistsByFromState(StateName))
            {
                result = transitions.Where(x => x.FromState.Equals(StateName)).FirstOrDefault();
            }

            return result;
        }
        #endregion GetTransitionByFromState

        #region GetTransitionByToState
        private IStateTransition GetTransitionByToState(string StateName)
        {
            IStateTransition result = default;

            if (TransitionExistsByFromState(StateName))
            {
                result = transitions.Where(x => x.ToState.Equals(StateName)).FirstOrDefault();
            }

            return result;
        }
        #endregion GetTransitionByToState

        #region ToMermaid
        public string ToMermaid()
        {
            bool useGraphForTransitionDetail = true;  // use the graph output for adding transition names
            string result;

            // add layout type
            if (useGraphForTransitionDetail)
            {
                result = "graph LR\n";  // standard type, see https://mermaid-js.github.io/mermaid/#/flowchart?id=graph for more detail
            }
            else
            {
                result = "stateDiagram-v2\n";  // standard type, see https://mermaid-js.github.io/mermaid/#/flowchart?id=graph for more detail
            }

            // add nodes and transitions by only using transition information
            foreach (IStateTransition transition in transitions)
            {
                string newLine = string.Empty;
                if (useGraphForTransitionDetail)
                {
                    newLine = $"{transition.FromState} -- {transition.Name} --> {transition.ToState}\n";
                }
                else
                {
                    newLine = $"{transition.FromState} --> {transition.ToState}\n";
                }
                result += newLine;
            }

            return result;
        }
        #endregion ToMermaid

        #region FromMermaid
        public void FromMermaid(string Input)
        {
            Clear();

            string[] lines = Input.Split('\n');

            foreach (string line in lines)
            {
                ParseFromMermaidLine(line);
            }
        }
        #endregion FromMermaid

        #region ParseFromMermaidLine
        private void ParseFromMermaidLine(string line)
        {
            //ignore first line if it starts with "flowchart" or "graph" -> does not fall into below patterns
            if (line.Contains(" --> ") && !line.Contains(" -- "))
            {
                ParseSimpleLineFromMermaid(line);
            }
            else if (line.Contains(" --> ") && line.Contains(" -- "))
            {
                //e.g.:  A -- text --> B -- text2 --> C
                ParseSimpleLineWithTransitionNameFromMermaid(line);
            }
        }
        #endregion ParseFromMermaidLine

        #region ParseSimpleLineFromMermaid
        private void ParseSimpleLineFromMermaid(string line)
        {
            // could be 'a --> b' 
            // could be 'a --> b --> c'
            logService.Debug("StateMachine", "ParseSimpleLineFromMermaid", $"Found direct state connection '{line}'");
            // we have several states connected
            // generate transition name
            // extract state names
            string[] stateNames = line.Split(new string[] { "-->" }, StringSplitOptions.None);
            int addedStates = 0;
            int addedTransitions = 0;
            for (int i = 0; i < stateNames.Length; i++)
            {
                string cleanedName = stateNames[i].Trim();
                if (cleanedName.Contains("&"))
                {
                    // actually add several states
                    string[] linkedStates = cleanedName.Split(new string[] { "&" }, StringSplitOptions.None);
                    foreach (string linkedState in linkedStates)
                    {
                        //TODO: remove double code
                        string cleanedLinkedState = linkedState.Trim();
                        // add state
                        if (!states.Any(x => x.Name.Equals(cleanedLinkedState)))
                        {
                            AddState(cleanedLinkedState);
                            addedStates++;
                        }

                        // add transitions
                        if (i > 0 && i < stateNames.Length - 1)
                        {
                            // from the state
                            string fromTransitionName = $"{stateNames[i - 1].Trim()}To{cleanedLinkedState}";
                            if (!transitions.Any(x => x.Name.Equals(fromTransitionName)))
                            {
                                AddTransition(fromTransitionName, stateNames[i - 1].Trim(), cleanedLinkedState);
                                addedTransitions++;
                            }

                            // to the next state
                            string toTransitionName = $"{cleanedLinkedState}To{stateNames[i + 1].Trim()}";
                            if (!transitions.Any(x => x.Name.Equals(toTransitionName)))
                            {
                                AddTransition(toTransitionName, cleanedLinkedState, stateNames[i + 1].Trim());
                                addedTransitions++;
                            }
                        }
                    }
                }
                else
                {
                    // add state
                    if (!states.Any(x => x.Name.Equals(cleanedName)))
                    {
                        AddState(cleanedName);
                        addedStates++;
                    }

                    // add transition
                    if (i > 0)
                    {
                        if (!stateNames[i - 1].Contains("&"))  // do not use the following logic when the successor is an added up state
                        {
                            string transitionName = $"{stateNames[i - 1].Trim()}To{cleanedName}";
                            if (!transitions.Any(x => x.Name.Equals(transitionName)))
                            {
                                AddTransition(transitionName, stateNames[i - 1].Trim(), cleanedName);
                                addedTransitions++;
                            }
                        }
                    }

                }
            }
            logService.Debug("StateMachine", "ParseSimpleLineFromMermaid", $"Added '{addedStates}' states and '{addedTransitions}' transitions");
        }
        #endregion ParseSimpleLineFromMermaid

        #region ParseSimpleLineWithTransitionNameFromMermaid
        private void ParseSimpleLineWithTransitionNameFromMermaid(string line)
        {
            // the '&' operator in mermaid syntax is not supported as 'a -- SomeTranition --> b & c' does not make sense as actually two transitions are generated - how should they be named anyway?
            logService.Debug("StateMachine", "ParseSimpleLineWithTransitionNameFromMermaid", $"Found direct state connection with transition name '{line}'");

            string[] stateAndTransitionNames = line.Split(new string[] { "-->", "--" }, StringSplitOptions.None);
            int addedStates = 0;
            int addedTransitions = 0;
            bool isState = true;
            for (int i = 0; i < stateAndTransitionNames.Length; i++)
            {
                string cleanedName = stateAndTransitionNames[i].Trim();
                if (isState)
                {
                    // add state
                    if (!states.Any(x => x.Name.Equals(cleanedName)))
                    {
                        AddState(cleanedName);
                        addedStates++;
                    }
                }
                else
                {
                    // just add one transition
                    if (!transitions.Any(x => x.Name.Equals(cleanedName)))
                    {
                        AddTransition(cleanedName, stateAndTransitionNames[i - 1].Trim(), stateAndTransitionNames[i + 1].Trim());
                        addedTransitions++;
                    }

                }

                isState = !isState;
            }
            logService.Debug("StateMachine", "ParseSimpleLineWithTransitionNameFromMermaid", $"Added '{addedStates}' states and '{addedTransitions}' transitions");
        }
        #endregion ParseSimpleLineWithTransitionNameFromMermaid

        #region Clear
        public void Clear()
        {
            logService.Info("StateMachine", "Clear", $"Attempting to clear StateMachine with States '{string.Join(", ", states)}'");
            states.Clear();
            transitions.Clear();
            logService.Info("StateMachine", "Clear", $"Cleared StateMachine");
        }
        #endregion Clear

        #endregion Methods

        #region Events

        #endregion Events
    }
}
