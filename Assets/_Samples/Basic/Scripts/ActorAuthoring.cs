using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class ActorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public Actor Actor;
    public BasicStateMachine BasicStateMachine;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, Actor);
        dstManager.AddComponentData(entity, BasicStateMachine);
    }
}
