using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[Serializable]
public struct BasicStateMachine : IComponentData
{
	public enum State
	{
		StateMove,
		StateRotate,
	}

	public State CurrentState;
	public State PreviousState;

	public StateMove StateMove;
	public StateRotate StateRotate;

	public void TransitionTo(State newState, ref Actor actor, bool force = false)
	{
		if (force || CurrentState != newState)
		{
			PreviousState = CurrentState;
			CurrentState = newState;
			OnStateExit(PreviousState, ref actor);
			OnStateEnter(CurrentState, ref actor);
		}
	}

	public void OnStateEnter(State state, ref Actor actor)
	{
		switch (state)
		{
			case State.StateMove:
				StateMove.OnStateEnter(ref actor);
				break;
			case State.StateRotate:
				StateRotate.OnStateEnter(ref actor);
				break;
		}
	}

	public void OnStateExit(State state, ref Actor actor)
	{
		switch (state)
		{
			case State.StateMove:
				StateMove.OnStateExit(ref actor);
				break;
			case State.StateRotate:
				StateRotate.OnStateExit(ref actor);
				break;
		}
	}

	public void OnStateUpdate(State state, Single deltaTime, ref Actor actor, ref BasicStateMachine stateMachine, ref Translation translation, ref Rotation rotation)
	{
		switch (state)
		{
			case State.StateMove:
				StateMove.OnStateUpdate(deltaTime, ref actor, ref stateMachine, ref translation, ref rotation);
				break;
			case State.StateRotate:
				StateRotate.OnStateUpdate(deltaTime, ref actor, ref stateMachine, ref translation, ref rotation);
				break;
		}
	}
}
