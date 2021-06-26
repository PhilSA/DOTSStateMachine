using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[Serializable]
public struct StateRotate : IBasicSMState
{
    public float Duration;
    public float RotationSpeed;
    public float3 RotationAxis;

    [NonSerialized]
    public float DurationCounter;

    public void OnStateEnter(ref Actor actor)
    {
        DurationCounter = Duration;
    }

    public void OnStateExit(ref Actor actor)
    {
    }

    public void OnStateUpdate(float deltaTime, ref Actor actor, ref BasicStateMachine stateMachine, ref Translation translation, ref Rotation rotation)
    {
        rotation.Value = math.mul(rotation.Value, quaternion.Euler(math.normalizesafe(RotationAxis) * RotationSpeed * actor.RotationSpeedMultiplier * deltaTime));

        if(DurationCounter <= 0f)
        {
            stateMachine.TransitionTo(BasicStateMachine.State.StateMove, ref actor);
        }
        DurationCounter -= deltaTime;
    }
}
