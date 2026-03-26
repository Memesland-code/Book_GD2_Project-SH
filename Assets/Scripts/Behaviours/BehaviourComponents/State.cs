using System;
using Unity.Behavior;

[BlackboardEnum]
public enum State
{
	Patrol,
	Chase,
	Investigate,
	ReturnToSpawn,
	Attack,
	Dead
}
