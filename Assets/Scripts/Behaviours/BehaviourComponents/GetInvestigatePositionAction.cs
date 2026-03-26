using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Unity.VisualScripting;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Get investigate position", story: "Assign [Target] position to [InvestigatePosition] using [InvestigatePositionScript]", category: "Action", id: "8465b430f7c2613c50ae1aac1a2f413b")]
public partial class GetInvestigatePositionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<Vector3> InvestigatePosition;
    [SerializeReference] public BlackboardVariable<GetInvestigatePosition> InvestigatePositionScript;
    protected override Status OnUpdate()
    {
	    InvestigatePosition.Value = InvestigatePositionScript.Value.GetTargetPosition(Target.Value);
	    
        return InvestigatePosition.Value == null ? Status.Failure : Status.Success;
    }
}

