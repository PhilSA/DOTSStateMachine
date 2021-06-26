using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class ActorSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((Entity entity, ref Actor actor, ref BasicStateMachine stateMachine, ref Translation translation, ref Rotation rotation) => 
        {
            stateMachine.OnStateUpdate(stateMachine.CurrentState, deltaTime, ref actor, ref stateMachine, ref translation, ref rotation);
        }).Schedule();
    }
}
