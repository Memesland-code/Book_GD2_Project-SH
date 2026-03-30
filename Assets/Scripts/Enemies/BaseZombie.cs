using Player;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class BaseZombie : MonoBehaviour, IDamageable, ISoundListener
{
	private static readonly int HitReaction = Animator.StringToHash("HitReaction");
	private static readonly int Revive1 = Animator.StringToHash("Revive");
	private static readonly int HeavyHitReaction = Animator.StringToHash("HeavyHitReaction");
	private static readonly int StaggerBlocked = Animator.StringToHash("StaggerBlocked");

	[SerializeField] private float maxHealth = 100f;
	[SerializeField] private float damageCooldown;
	private float nextDamageAcceptTime;
	[SerializeField] private int hitNumbersToStagger;
	private int currentHitTimes;
	[SerializeField] private float timeToExpireStagger;
	private float nextStaggerExpireTime;
	
	[SerializeField] private float hitDamage;
    private float currentHealth;
	    
    [SerializeField] protected BehaviorGraphAgent behaviorAgent;
    [SerializeField] private ResetGraphValues resetChannel;

    [SerializeField] private float reviveChance;
    private float baseReviveChance;
    [SerializeField] private float reviveChanceAddOnCollision;

    [SerializeField] protected AudioSource zombieAudioSource;
    [SerializeField] private AudioClip zombieScreamClip;
    [SerializeField] private AudioClip zombieHeavyHitClip;
    
    protected BlackboardVariable<EnemyBehaviourStates> bbCurrentState;
    protected BlackboardVariable<Vector3> bbInvestigatePosition;
    private BlackboardVariable<GameObject> bbTarget;
    private BlackboardVariable<bool> bbForceChasePlayer;
    
    private Animator animator;
    protected bool isDead;
    protected Vector3 currentSoundPosition;

    private Collider deadTrigger;

    private void Awake()
    {
	    currentHealth = maxHealth;
	    animator = GetComponent<Animator>();
	    deadTrigger = GetComponent<SphereCollider>();
	    deadTrigger.enabled = false;
	    baseReviveChance = reviveChance;
    }

    public virtual void Start()
    {
	    
	    behaviorAgent = GetComponent<BehaviorGraphAgent>();
	    
	    // Check if BB reference exists and set them to our variable
	    if (behaviorAgent.BlackboardReference.GetVariable("EnemyBehaviourStates", out bbCurrentState)) {}
	    if (behaviorAgent.BlackboardReference.GetVariable("InvestigatePosition", out bbInvestigatePosition)) {}
	    if (behaviorAgent.BlackboardReference.GetVariable("Target", out bbTarget)) {}
	    if (behaviorAgent.BlackboardReference.GetVariable("ForceChasePlayer", out bbForceChasePlayer)) {}
    }

    
    
    //* ===== IDamageable and attack Related =====
    public void TakeDamage(float damageAmount, GameObject damageSource)
    {
	    if (isDead) return;
	    if (Time.time <= nextDamageAcceptTime) return;
	    
	    nextDamageAcceptTime = Time.time + damageCooldown;
	    
	    currentHealth -= damageAmount;

	    if (currentHealth <= 0)
	    {
		    Death();
		    return;
	    }
	    
	    if (damageSource.CompareTag("Player"))
	    {
		    bbForceChasePlayer.Value = true;
		    bbTarget.Value = damageSource;
		    bbCurrentState.Value = EnemyBehaviourStates.Chase;
	    }

	    if (Time.time > nextStaggerExpireTime)
	    {
		    currentHitTimes = 0;
	    }

	    currentHitTimes++;
	    nextStaggerExpireTime = Time.time + timeToExpireStagger;
	    
	    if (currentHitTimes >= hitNumbersToStagger)
	    {
		    bbCurrentState.Value = EnemyBehaviourStates.Stagger;
		    animator.SetBool(StaggerBlocked, true);
		    animator.SetTrigger(HeavyHitReaction);
		    currentHitTimes = 0;
		    return;
	    }
	    
	    if (damageSource.TryGetComponent(out PlayerController player) && (int)damageAmount == (int)player.stabDamage)
	    {
		    player.DamageReceivedByTarget(true);
		    animator.SetTrigger(HitReaction);
	    }
    }

    public void HeavyHitReactionFinished()
    {
	    animator.SetBool(StaggerBlocked, false);
	    
	    if (isDead) return;
	    bbCurrentState.Value = EnemyBehaviourStates.ReturnToSpawn;
    }

    public void Heal(float healAmount)
    {
	    currentHealth = Mathf.Clamp(currentHealth + healAmount, 0, 100);
    }

    private void Death()
    {
	    isDead = true;
	    
	    GetComponent<Rigidbody>().isKinematic = true;
	    GetComponent<CapsuleCollider>().enabled = false;
	    
	    bbForceChasePlayer.Value = false;
	    bbTarget.Value = null;
	    bbCurrentState.Value = EnemyBehaviourStates.Dead;
	    
	    deadTrigger.enabled = true;
	    
    }

    // Effective revive to animate zombie
    public void Revive()
    {
	    deadTrigger.enabled = false;
	    reviveChance = baseReviveChance;
	    resetChannel.SendEventMessage();
    }

    // Called on animation ended
    public void ReviveEnded()
    {
	    GetComponent<Rigidbody>().isKinematic = true;
	    GetComponent<CapsuleCollider>().enabled = true;
	    currentHealth = maxHealth;
	    isDead = false;
    }

    public void OnAttackCollision(Collider otherCollider)
    {
	    if (otherCollider.CompareTag("Player"))
	    {
		    otherCollider.GetComponent<IDamageable>().TakeDamage(hitDamage, gameObject);
	    }
    }
    
    public void AttackPlayer(int isStart)
    {
	    GetComponentInChildren<HitDetectZone>().gameObject.GetComponent<BoxCollider>().enabled = isStart == 1;
    }
    
    

    //* ===== Senses =====
    public virtual void OnSoundHeard(Vector3 soundPosition, GameObject source)
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
	    
	    bbCurrentState.Value = EnemyBehaviourStates.Patrol; // Workaround, forces graph blackboard to be reevaluated
	    bbCurrentState.Value = EnemyBehaviourStates.Investigate;
    }

    public void OnSoundInvestigate()
    {
	    currentSoundPosition = Vector3.zero;
    }


    private void OnCollisionEnter(Collision other)
    {
	    if (other.gameObject.CompareTag("Player") && !isDead)
	    {
		    bbForceChasePlayer.Value = true;
		    bbTarget.Value = other.gameObject;
		    bbCurrentState.Value = EnemyBehaviourStates.Chase;
	    }
    }

    // The more the player get close to the dead zombie, the more it will have chances to revive
    private void OnTriggerEnter(Collider other)
    {
	    if (other.CompareTag("Player") && isDead)
	    {
		    float rolledReviveChances = Random.Range(0, 100);

		    if (rolledReviveChances <= reviveChance)
		    {
			    Revive();
			    Debug.LogWarning("Revived because of player");
		    }
		    else
		    {
			    reviveChance += reviveChanceAddOnCollision;
		    }
	    }
    }
    
    
    
    //* ===== Others - Sound effects =====
    public void PlayScreamSound()
    {
	    zombieAudioSource.clip = zombieScreamClip;
	    zombieAudioSource.Play();
    }

    public void PlayHeavyHitReactionSound()
    {
	    zombieAudioSource.clip = zombieHeavyHitClip;
	    zombieAudioSource.Play();
    }
}
