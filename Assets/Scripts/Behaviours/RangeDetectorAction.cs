using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

namespace Behaviours
{
	[Serializable, GeneratePropertyBag]
	[NodeDescription(name: "RangeDetector", story: "Update Range [Detector] and assign [target]", category: "Action", id: "ef09657f366485f34324ba6a05c5a4f2")]
	public partial class RangeDetectorAction : Action
	{
		[SerializeReference] public BlackboardVariable<RangeDetector> Detector;
		[SerializeReference] public BlackboardVariable<GameObject> Target;

		protected override Status OnUpdate()
		{
			Target.Value = Detector.Value.UpdateDetector();
	    
			return Target.Value == null ? Status.Failure : Status.Success;
		}
	}
}

