using System;
using Behaviours;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Update Override Range Check", story: "Updates [OverrideRangeCheck] and assign [Target]", category: "Action", id: "af881002b55738497d2807177da43844")]
public partial class UpdateOverrideRangeCheckAction : Action
{
	[SerializeReference] public BlackboardVariable<OverrideCloseRangeDetector> OverrideRangeCheck;
	[SerializeReference] public BlackboardVariable<GameObject> Target;

	protected override Status OnUpdate()
	{
		return OverrideRangeCheck.Value.UpdateOverrideRangeDetector() == null ? Status.Failure : Status.Success;
	}
}

