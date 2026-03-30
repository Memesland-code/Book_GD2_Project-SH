using Unity.Behavior;
using UnityEngine;

public class SoundSensitiveZombie : BaseZombie
{
	[Space(20), Header("Subclass parameters")]
	[SerializeField] private float radialAttackDamage;
	[SerializeField] private AudioClip radialGrowlSound;
	private BlackboardVariable<bool> bbIsSoundSensitive;

	public override void Start()
	{
		base.Start();
		
		if (behaviorAgent.BlackboardReference.GetVariable("IsSoundSensitive", out bbIsSoundSensitive)) {}
	}
	
	public void EnableRadialAttackCollider(int isStart)
	{
		GetComponentInChildren<HitDetectZone>().gameObject.GetComponent<BoxCollider>().enabled = isStart == 1;

		if (isStart == 1)
		{
			zombieAudioSource.clip = radialGrowlSound;
			zombieAudioSource.Play();
		}
	}
	
	public override void OnSoundHeard(Vector3 soundPosition, GameObject source)
	{
		if (isDead) return;
	    
		if (currentSoundPosition == Vector3.zero)
		{
			currentSoundPosition = soundPosition;
			bbInvestigatePosition.Value = soundPosition;
		}
		else
		{
			float currentSoundDistance = Vector3.Distance(transform.position, currentSoundPosition);
			float newSoundDistance = Vector3.Distance(transform.position, soundPosition);

			if (newSoundDistance < currentSoundDistance)
			{
				currentSoundPosition = soundPosition;
				bbInvestigatePosition.Value = soundPosition;
			}
		}
	    
		
		bbIsSoundSensitive.Value = true;
		bbCurrentState.Value = EnemyBehaviourStates.Patrol; // Workaround, forces graph blackboard to be reevaluated
		bbCurrentState.Value = EnemyBehaviourStates.Investigate;
	}
}
