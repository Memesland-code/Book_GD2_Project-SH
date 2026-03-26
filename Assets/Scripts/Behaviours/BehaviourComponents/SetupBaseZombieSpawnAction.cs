using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Setup Base Zombie Spawn", story: "Assign [BasePosition] and [BaseRotation] using [Self]", category: "Action", id: "95c064fafa836082ee25323e33193801")]
public partial class SetupBaseZombieSpawnAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> BasePosition;
    [SerializeReference] public BlackboardVariable<Vector3> BaseRotation;
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    protected override Status OnUpdate()
    {
	    BasePosition.Value = Self.Value.transform.position;
	    BaseRotation.Value = Self.Value.transform.rotation.eulerAngles;

	    return Status.Success;
    }
}

