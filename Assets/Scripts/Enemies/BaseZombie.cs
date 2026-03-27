using Player;
using Unity.Behavior;
using UnityEngine;

public class BaseZombie : MonoBehaviour, IDamageable, ISoundListener
{
	private static readonly int HitReaction = Animator.StringToHash("HitReaction");
	private static readonly int Revive1 = Animator.StringToHash("Revive");
	
	[SerializeField] private float maxHealth = 100f;
	[SerializeField] private float damageCooldown;
	private float nextDamageAcceptTime;
	
	[SerializeField] private float hitDamage;
    private float currentHealth;
	    
    [SerializeField] private BehaviorGraphAgent behaviorAgent;
    [SerializeField] private ResetGraphValues resetChannel;
    
    private BlackboardVariable<State> bbCurrentState;
    private BlackboardVariable<Vector3> bbInvestigatePosition;
    private BlackboardVariable<GameObject> bbTarget;
    private BlackboardVariable<bool> bbForceChasePlayer;
    
    private Animator animator;
    private bool isDead;
    private Vector3 currentSoundPosition;

    private void Awake()
    {
	    currentHealth = maxHealth;
	    animator = GetComponent<Animator>();
    }

    private void Start()
    {
	    behaviorAgent = GetComponent<BehaviorGraphAgent>();
	    
	    // Check if BB reference exist and set them to our variable
	    if (behaviorAgent.BlackboardReference.GetVariable("CurrentState", out bbCurrentState)) {}
	    if (behaviorAgent.BlackboardReference.GetVariable("InvestigatePosition", out bbInvestigatePosition)) {}
	    if (behaviorAgent.BlackboardReference.GetVariable("Target", out bbTarget)) {}
	    if (behaviorAgent.BlackboardReference.GetVariable("ForceChasePlayer", out bbForceChasePlayer)) {}
    }

    public void AttackPlayer(int isStart)
    {
	    GetComponentInChildren<HitDetectZone>().gameObject.GetComponent<BoxCollider>().enabled = isStart == 1;
    }

    
    
    // ===== IDamageable Related =====
    public void TakeDamage(float damageAmount, GameObject damageSource)
    {
	    if (isDead || Time.time <= nextDamageAcceptTime) return;
	    
	    nextDamageAcceptTime = Time.time + damageCooldown;

	    if (damageSource.TryGetComponent(out PlayerController player))
		    player.DamageReceived();
	    
	    currentHealth -= damageAmount;

	    if (currentHealth <= 0)
	    {
		    Death();
		    return;
	    }
	    
	    animator.SetTrigger(HitReaction);

	    if (damageSource.CompareTag("Player"))
	    {
		    bbForceChasePlayer.Value = true;
		    bbTarget.Value = damageSource;
		    bbCurrentState.Value = State.Chase;
	    }
    }

    public void Heal(float healAmount)
    {
	    currentHealth = Mathf.Clamp(currentHealth + healAmount, 0, 100);
    }

    private void Death()
    {
	    GetComponent<Rigidbody>().isKinematic = true;
	    GetComponent<Collider>().enabled = false;
	    bbCurrentState.Value = State.Dead;
	    isDead = true;   
	    Debug.LogWarning(gameObject.name + " is Dead");
    }

    public void Revive()
    {
	    Debug.LogWarning("Revive executed on " + gameObject.name);
	    animator.SetTrigger(Revive1);
    }

    public void ReviveEnded()
    {
	    GetComponent<Rigidbody>().isKinematic = true;
	    GetComponent<Collider>().enabled = true;
	    currentHealth = maxHealth;
	    isDead = false;
	    resetChannel.SendEventMessage();
	    bbCurrentState.Value = State.Patrol;
    }

    public void OnAttackCollision(Collider otherCollider)
    {
	    if (otherCollider.CompareTag("Player"))
	    {
		    otherCollider.GetComponent<IDamageable>().TakeDamage(hitDamage, gameObject);
	    }
    }

    public void OnSoundHeard(Vector3 soundPosition, GameObject source)
    {
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
	    
	    bbCurrentState.Value = State.Patrol; // Workaround, forces graph blackboard to be reevaluated
	    bbCurrentState.Value = State.Investigate;
    }

    public void OnSoundInvestigate()
    {
	    currentSoundPosition = Vector3.zero;
    }
}
