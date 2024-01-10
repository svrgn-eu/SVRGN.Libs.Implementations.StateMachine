[TOC]

# About

This package contains an implementation of a state machine following https://github.com/svrgn-eu/SVRGN.Libs.Contracts.StateMachine. It is a merely string based implementation supporting a wide variety of state events, like entering, leaving and updating.

String based implementation helps on the one-hand side to avoid some overhead but also to interact with less dependencies, on the other hand you need to know what you're doing and you surely should avoid magic strings (the "magic" is misleading ;-) https://en.wikipedia.org/wiki/Magic_string.

# How to use
You can find all test cases in https://github.com/svrgn-eu/SVRGN.Libs.Implementations.StateMachine/tree/main/src/SVRGN.Libs.Implementations.StateMachine.Tests.

Let's go over the basic usage here:

1. Instantiate (either manually or via Dependency Injection)
2. Set up states and transitions
3. Optional: set up actions for available events
4. Set starting state
5. use

The idea is to use transitions to move from state to state. The implementation then will check if that is possible and call the necessary events.

```csharp
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
```

One good idea could be that you name the transition by the states it transitions in between, e.g. from Start to Process = Start2Process and similar.
