using System;
using Behaviours;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Line of Sight check", story: "Check [Target] with [LineOfSight]", category: "Conditions", id: "4b4c9753b2e079d29ac5845a5912a9a9")]
public partial class LineOfSightCheckCondition : Condition
{
	[SerializeReference] public BlackboardVariable<GameObject> Target;
	[SerializeReference] public BlackboardVariable<LineOfSightDetector> LineOfSight;

	public override bool IsTrue()
	{
		return LineOfSight.Value.FilterDetectedTarget(Target.Value);
	}
}
