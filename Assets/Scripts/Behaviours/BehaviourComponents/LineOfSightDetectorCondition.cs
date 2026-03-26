using Behaviours;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Line of Sight Detector", story: "Check [Target] with [LineOfSightDetector] with sightState [SightState] and angleState [AngleState]", category: "Conditions", id: "00ba4fc175dca58927b2ea5737829d4d")]
public partial class LineOfSightDetectorCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<LineOfSightDetector> LineOfSightDetector;
    [SerializeReference] public BlackboardVariable<bool> SightState;
    [SerializeReference] public BlackboardVariable<bool> AngleState;

    public override bool IsTrue()
    {
	    return LineOfSightDetector.Value.FilterDetectedTarget(Target.Value, SightState.Value, AngleState.Value);
    }
}
