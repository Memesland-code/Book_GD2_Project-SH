using Behaviours;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Unity.VisualScripting.FullSerializer;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RangeDetector", story: "Update Range [Detector] and Assign [Target]", category: "Action", id: "b2f70e735eee95ca42158539e3dbbda1")]
public partial class RangeDetectorAction : Action
{
    [SerializeReference] public BlackboardVariable<RangeDetector> Detector;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    protected override Status OnUpdate()
    {
	    // Primitive check of player range
	    Target.Value = Detector.Value.UpdateDetector();
	    
	    if (Target.Value != null)
	    {
		    // Filter to real check if player can be seen
		    Target.Value = Detector.Value.FilterDetectedTarget(Target.Value);
	    }
	    
	    return Target.Value == null ? Status.Failure : Status.Success;
    }
}

