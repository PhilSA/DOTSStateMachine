
# DOTS State Machine

## How to install
Simply download the .unitypackage in the "Releases" section of this repository, and import into your DOTS-compatible project (requires Entities package)


## How to use

### Create the State Machine Definition
Create an interface that has the `StateMachineDefinition` attribute. This interface will define the functions that will be common to all states in your state machine. See an example [here](https://github.com/PhilSA/DOTSStateMachine/blob/master/Assets/_Samples/Basic/Scripts/StateMachine/BasicStateMachineDefinition.cs). You are allowed to pass any sort of parameters to those functions (including by "ref", "in", and "out"), but you cannot make those functions have a return type

The `StateMachineDefinition` attribute takes 3 parameters:
* the name of the generated state machine component
* the file path of the generated state machine component
* the additional usings that the generated state machine component should have

Moreover, there are two special-named functions that you can add to your interface:
* OnStateEnter: this will automatically get called whenever you transition to a new state by using the `TransitionTo` function of the generated state machine
* OnStateExit: this will automatically get called whenever you transition out of a state by using the `TransitionTo` function of the generated state machine

### Create the states
Create state structs implementing that interface. All structs implementing the same `StateMachineDefinition` interface will represent the various states that this specific state machine can have. See an example [here](https://github.com/PhilSA/DOTSStateMachine/blob/master/Assets/_Samples/Basic/Scripts/StateMachine/StateMove.cs) and [here](https://github.com/PhilSA/DOTSStateMachine/blob/master/Assets/_Samples/Basic/Scripts/StateMachine/StateRotate.cs)

### Generate the statemachine code
In the Unity editor, in the top menu bar, select "Tools > Generate StateMachines". This will generate the state machine component code with the name & patch defined in your `StateMachineDefinition` attribute.

You only really have to generate again when you either add new states, or add new functions to your state machine interface

You can now create an authoring component that adds this state machine component to an entity ([example](https://github.com/PhilSA/DOTSStateMachine/blob/master/Assets/_Samples/Basic/Scripts/ActorAuthoring.cs)), and call updates on you state machine with a system ([example](https://github.com/PhilSA/DOTSStateMachine/blob/master/Assets/_Samples/Basic/Scripts/ActorSystem.cs))
