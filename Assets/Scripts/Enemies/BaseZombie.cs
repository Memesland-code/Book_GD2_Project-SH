using Unity.Behavior;
using UnityEngine;

public class BaseZombie : MonoBehaviour, IDamageable, ISoundListener
{
	private static readonly int HitReaction = Animator.StringToHash("HitReaction");
	[SerializeField] private float maxHealth = 100f;
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
	    bool doStart = isStart == 1;
	    
	    GetComponentInChildren<HitDetectZone>().gameObject.GetComponent<BoxCollider>().enabled = doStart;
    }

    
    
    // ===== IDamageable Related =====
    public void TakeDamage(float damageAmount, GameObject damageSource)
    {
	    if (isDead) return;
	    
	    currentHealth -= damageAmount;

	    if (currentHealth <= 0)
	    {
		    bbCurrentState.Value = State.Dead;
			isDead = true;   
		    Debug.LogWarning(gameObject.name + " is Dead");
		    Revive();
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

    public void Revive()
    {
	    Debug.LogWarning("Revive executed on " + gameObject.name);
	    currentHealth = maxHealth;
	    isDead = false;
	    resetChannel.SendEventMessage();
	    bbCurrentState.Value = State.Patrol;
    }

    public void RunTriggerDetection(Collider otherCollider)
    {
	    if (otherCollider.CompareTag("Player"))
	    {
		    otherCollider.GetComponent<IDamageable>().TakeDamage(hitDamage, gameObject);
	    }
    }

    public void OnSoundHeard(Vector3 soundPosition, GameObject source)
    {
	    bbInvestigatePosition.Value = soundPosition;
	    bbCurrentState.Value = State.Investigate;
	    //! Deal with behavior graph to go investigate on sound position
	    //! Check if other sound heard ==> if still investigating ==> compare distance ==> go to nearest
    }
}
