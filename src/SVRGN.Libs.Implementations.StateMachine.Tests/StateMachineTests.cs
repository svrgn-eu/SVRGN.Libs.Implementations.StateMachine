using Microsoft.VisualStudio.TestTools.UnitTesting;
using SVRGN.Libs.Contracts.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SVRGN.Libs.Implementations.StateMachine.Tests
{
    [TestClass]
    [TestCategory("IStateMachine")]
    public class StateMachineTests : BaseTest
    {
        #region Methods

        #region GetCurrentState
        [TestMethod]
        public void GetCurrentState()
        {
            IStateMachine stateMachine = this.ObjectService.Create<IStateMachine>();

            int TransitionCounter = 0;

            stateMachine.AddState("Start");
            stateMachine.AddState("Process");
            stateMachine.AddState("End");
            stateMachine.StartWith("Start");
            stateMachine.AddTransition("Transition1", "Start", "Process");
            stateMachine.AddTransitionAction("Transition1", () => { TransitionCounter++; });
            stateMachine.AddTransition("Transition2", "Process", "End");
            stateMachine.AddTransitionAction("Transition2", () => { TransitionCounter++; });

            string firstState = stateMachine.GetCurrentStateName();
            stateMachine.Transition("Transition1");
            string secondState = stateMachine.GetCurrentStateName();

            Assert.AreEqual("Start", firstState);
            Assert.AreEqual("Process", secondState);
            Assert.AreEqual(1, TransitionCounter);
        }
        #endregion GetCurrentState

        #region Transition
        [TestMethod]
        public void Transition()
        {
            IStateMachine stateMachine = this.ObjectService.Create<IStateMachine>();  //Dependency Injection resolve call

            int TransitionCounter = 0;

            stateMachine.AddState("Start");  // add state "Start"
            stateMachine.AddState("Process");  // add state "Process"
            stateMachine.AddState("End");  // add state "End"
            stateMachine.StartWith("Start");
            stateMachine.AddTransition("Start2Process", "Start", "Process");  // add transition possibility from "Start" to "Process"
            stateMachine.AddTransitionAction("Start2Process", () => { TransitionCounter++; });  // adds an action which is called when calling that transition
            stateMachine.AddTransition("Process2End", "Process", "End");  // add transition possibility from "Process" to "End"
            stateMachine.AddTransitionAction("Process2End", () => { TransitionCounter++; });

            bool hasTransitionHappened = stateMachine.Transition("Transition1");  // perform the transition and check, if it happened

            Assert.AreEqual(true, hasTransitionHappened);
            Assert.AreEqual(1, TransitionCounter);
        }
        #endregion Transition

        #region TransitionTo
        [DataTestMethod]
        [DataRow("Start", "Process", true)]
        [DataRow("Start", "End", false)]
        public void TransitionTo(string StartStateName, string TargetStateName, bool IsExpectedToBeSuccessful)
        {
            IStateMachine stateMachine = this.ObjectService.Create<IStateMachine>();

            int TransitionCounter = 0;

            stateMachine.AddState("Start");
            stateMachine.AddState("Process");
            stateMachine.AddState("End");
            stateMachine.StartWith(StartStateName);
            stateMachine.AddTransition("Start2Process", "Start", "Process");  // add transition possibility from "Start" to "Process"
            stateMachine.AddTransitionAction("Start2Process", () => { TransitionCounter++; });  // adds an action which is called when calling that transition
            stateMachine.AddTransition("Process2End", "Process", "End");  // add transition possibility from "Process" to "End"
            stateMachine.AddTransitionAction("Process2End", () => { TransitionCounter++; });

            bool hasTransitionHappened = stateMachine.TransitionTo(TargetStateName);

            if (IsExpectedToBeSuccessful)
            {
                Assert.AreEqual(true, hasTransitionHappened);
                Assert.AreEqual(1, TransitionCounter);
            }
            else
            {
                Assert.AreEqual(false, hasTransitionHappened);
                Assert.AreEqual(0, TransitionCounter);
            }
        }
        #endregion TransitionTo

        #region TransitionWithoutAction
        [TestMethod]
        public void TransitionWithoutAction()
        {
            IStateMachine stateMachine = this.ObjectService.Create<IStateMachine>();

            stateMachine.AddState("Start");
            stateMachine.AddState("Process");
            stateMachine.AddState("End");
            stateMachine.StartWith("Start");
            stateMachine.AddTransition("Transition1", "Start", "Process");
            stateMachine.AddTransition("Transition2", "Process", "End");

            bool hasTransitionHappened = stateMachine.Transition("Transition1");

            Assert.AreEqual(true, hasTransitionHappened);
        }
        #endregion TransitionWithoutAction

        #region DoubleTransition
        [TestMethod]
        public void DoubleTransition()
        {
            IStateMachine stateMachine = this.ObjectService.Create<IStateMachine>();

            int TransitionCounter = 0;

            stateMachine.AddState("Start");
            stateMachine.AddState("Process");
            stateMachine.AddState("End");
            stateMachine.StartWith("Start");
            stateMachine.AddTransition("Transition1", "Start", "Process");
            stateMachine.AddTransitionAction("Transition1", () => { TransitionCounter++; });
            stateMachine.AddTransition("Transition2", "Process", "End");
            stateMachine.AddTransitionAction("Transition2", () => { TransitionCounter++; });

            bool hasTransition1Happened = stateMachine.Transition("Transition1");
            bool hasTransition2Happened = stateMachine.Transition("Transition2");

            Assert.AreEqual(true, hasTransition1Happened);
            Assert.AreEqual(true, hasTransition2Happened);
            Assert.AreEqual(2, TransitionCounter);
        }
        #endregion DoubleTransition

        #region TransitionNegative
        [TestMethod]
        public void TransitionNegative()
        {
            IStateMachine stateMachine = this.ObjectService.Create<IStateMachine>();

            int TransitionCounter = 0;

            stateMachine.AddState("Start");
            stateMachine.AddState("Process");
            stateMachine.AddState("End");
            stateMachine.StartWith("Start");
            stateMachine.AddTransition("Transition1", "Start", "Process");
            stateMachine.AddTransitionAction("Transition1", () => { TransitionCounter++; });
            stateMachine.AddTransition("Transition2", "Process", "End");
            stateMachine.AddTransitionAction("Transition2", () => { TransitionCounter++; });

            bool hasTransitionHappened = stateMachine.Transition("Transition2");  // transition cannot happen as start state is wrong

            Assert.AreEqual(false, hasTransitionHappened);
            Assert.AreEqual(0, TransitionCounter);
        }
        #endregion TransitionNegative

        #region TransitionMethodsDetailed
        [TestMethod]
        public void TransitionMethodsDetailed()
        {
            IStateMachine stateMachine = this.ObjectService.Create<IStateMachine>();

            int TransitionCounter = 0;

            stateMachine.AddState("Start");
            stateMachine.AddState("Process");
            stateMachine.AddState("End");

            stateMachine.AddStateEnterAction("Start", () => { TransitionCounter++; });
            stateMachine.AddStateUpdateAction("Start", () => { TransitionCounter++; });
            stateMachine.AddStateExitAction("Start", () => { TransitionCounter++; });
            stateMachine.AddStateEnterAction("Process", () => { TransitionCounter++; });
            stateMachine.AddStateUpdateAction("Process", () => { TransitionCounter++; });
            stateMachine.AddStateExitAction("Process", () => { TransitionCounter++; });
            stateMachine.AddStateEnterAction("End", () => { TransitionCounter++; });
            stateMachine.AddStateUpdateAction("End", () => { TransitionCounter++; });
            stateMachine.AddStateExitAction("End", () => { TransitionCounter++; });

            stateMachine.StartWith("Start");

            stateMachine.AddTransition("Transition1", "Start", "Process");
            stateMachine.AddTransitionAction("Transition1", () => { TransitionCounter++; });

            stateMachine.AddTransition("Transition2", "Process", "End");
            stateMachine.AddTransitionAction("Transition2", () => { TransitionCounter++; });

            bool hasTransition1Happened = stateMachine.Transition("Transition1");
            bool hasTransition2Happened = stateMachine.Transition("Transition2");

            stateMachine.Update();

            Assert.AreEqual(true, hasTransition1Happened);
            Assert.AreEqual(true, hasTransition2Happened);
            Assert.AreEqual(8, TransitionCounter);  // 1 StartsWith, 2*State + 1*Transition with Transition1, 2*State + 1*Transition with Transition2, 1* Update
        }
        #endregion TransitionMethodsDetailed

        #region TransitionStartAndExitActions
        [TestMethod]
        public void TransitionStartAndExitActions()
        {
            IStateMachine stateMachine = this.ObjectService.Create<IStateMachine>();

            int TransitionCounter = 0;

            stateMachine.AddState("Start");
            stateMachine.AddState("Process");
            stateMachine.AddState("End");
            stateMachine.StartWith("Start");
            stateMachine.AddTransition("Transition1", "Start", "Process");
            stateMachine.AddTransitionEnterAction("Transition1", () => { TransitionCounter++; });
            stateMachine.AddTransitionAction("Transition1", () => { TransitionCounter++; });
            stateMachine.AddTransitionExitAction("Transition1", () => { TransitionCounter++; });
            stateMachine.AddTransition("Transition2", "Process", "End");
            stateMachine.AddTransitionEnterAction("Transition2", () => { TransitionCounter++; });
            stateMachine.AddTransitionAction("Transition2", () => { TransitionCounter++; });
            stateMachine.AddTransitionExitAction("Transition2", () => { TransitionCounter++; });

            bool hasTransitionHappened = stateMachine.Transition("Transition1");

            Assert.AreEqual(true, hasTransitionHappened);
            Assert.AreEqual(3, TransitionCounter);
        }
        #endregion TransitionStartAndExitActions

        #region ToMermaid
        [TestMethod]
        public void ToMermaid()
        {
            IStateMachine stateMachine = this.ObjectService.Create<IStateMachine>();

            stateMachine.AddState("Start");
            stateMachine.AddState("Process");
            stateMachine.AddState("ProcessParallel");
            stateMachine.AddState("End");
            stateMachine.StartWith("Start");
            stateMachine.AddTransition("Transition1", "Start", "Process");
            stateMachine.AddTransition("Transition11", "Start", "ProcessParallel");
            stateMachine.AddTransitionAction("Transition1", () => { });
            stateMachine.AddTransition("Transition2", "Process", "End");
            stateMachine.AddTransition("Transition21", "ProcessParallel", "End");
            stateMachine.AddTransitionAction("Transition2", () => { });

            string output = stateMachine.ToMermaid();

            string[] lines = output.Split('\n');
            int lineCount = lines.Length;

            Assert.AreEqual(false, string.IsNullOrEmpty(output));
            Assert.AreEqual(6, lineCount);
        }
        #endregion ToMermaid

        #region FromMermaid
        [DataTestMethod]
        [DataRow("graph LR\na --> b\nb --> a", 2, 2)]
        [DataRow("graph LR\na --> b --> a", 2, 2)]
        [DataRow("graph LR\nA -- text --> B -- text2 --> C", 3, 2)]
        [DataRow("graph LR\na --> b & c --> d", 4, 4)]  // read as "add a transition from state 'a' to 'b' and also from state 'a' to 'c'. Then create a transition from 'b' and 'c' to 'd'"
        public void FromMermaid(string Input, int ExpectedNumberOfStates, int ExpectedNumberOfTransitions)
        {
            IStateMachine stateMachine = this.ObjectService.Create<IStateMachine>();

            stateMachine.FromMermaid(Input);

            Assert.AreEqual(ExpectedNumberOfStates, stateMachine.StateCount);
            Assert.AreEqual(ExpectedNumberOfTransitions, stateMachine.TransitionCount);
        }
        #endregion FromMermaid

        #endregion Methods
    }
}
