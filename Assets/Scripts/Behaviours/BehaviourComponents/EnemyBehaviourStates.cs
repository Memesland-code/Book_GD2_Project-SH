using Unity.Behavior;

[BlackboardEnum]
public enum EnemyBehaviourStates
{
	Patrol = 0,
	Chase = 1,
	Investigate = 2,
	ReturnToSpawn = 3,
	Attack = 4,
	Dead = 5,
	Stagger = 6,
	RadialAttack = 7
}
