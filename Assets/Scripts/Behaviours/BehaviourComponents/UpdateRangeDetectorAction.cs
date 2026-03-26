using System;
using Behaviours;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Update RangeDetector", story: "Updates [RangeDetector] and assign [Target] with [Range] check", category: "Action", id: "ab9dfb1e4778af27050d707a5e1b0598")]
public partial class UpdateRangeDetectorAction : Action
{
    [SerializeReference] public BlackboardVariable<RangeDetector> RangeDetector;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Range;
	protected override Status OnUpdate()
	{
		if (RangeDetector.Value.UpdateDetector(Range) == null) return Status.Failure;
		
		Target.Value = RangeDetector.Value.UpdateDetector(Range);
		return Status.Success;
	}
}

