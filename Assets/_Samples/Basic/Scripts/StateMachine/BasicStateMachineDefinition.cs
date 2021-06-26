using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[StateMachineDefinition("BasicStateMachine", "/_Samples/Basic/_GENERATED", new string[] { "Unity.Transforms" })]
public interface IBasicSMState
{
    void OnStateEnter(ref Actor actor);
    void OnStateExit(ref Actor actor);
    void OnStateUpdate(float deltaTime, ref Actor actor, ref BasicStateMachine stateMachine, ref Translation translation, ref Rotation rotation);
}