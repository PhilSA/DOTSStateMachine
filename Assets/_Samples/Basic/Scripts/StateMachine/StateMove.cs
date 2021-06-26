using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[Serializable]
public struct StateMove : IBasicSMState
{
    public float Duration;
    public float MoveSpeed;
    public float3 MoveDirection;

    [NonSerialized]
    public float DurationCounter;

    public void OnStateEnter(ref Actor actor)
    {
        // Flip direction
        MoveDirection = -MoveDirection;

        DurationCounter = Duration;
    }

    public void OnStateExit(ref Actor actor)
    {
    }

    public void OnStateUpdate(float deltaTime, ref Actor actor, ref BasicStateMachine stateMachine, ref Translation translation, ref Rotation rotation)
    {
        translation.Value += MoveDirection * MoveSpeed * actor.MoveSpeedMultiplier * deltaTime;

        if (DurationCounter <= 0f)
        {
            stateMachine.TransitionTo(BasicStateMachine.State.StateRotate, ref actor);
        }
        DurationCounter -= deltaTime;
    }
}
