using System;
using System.Collections.Generic;
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
    [SerializeField] private float maxStuckTime;
    private float stuckTimer;
    
    [Header("DEMO ONLY"), Space(10)]
    [SerializeField] private bool testZombieSight;

    [SerializeField] private List<GameObject> wallsToChange;
    [SerializeField] private Material unseenMaterial;
    [SerializeField] private Material seenMaterial;
    private BlackboardVariable<bool> bbIsTargetDetected;

    private NavMeshAgent agent;

    private void Awake()
    {
	    currentHealth = maxHealth;
	    animator = GetComponent<Animator>();
	    deadTrigger = GetComponent<SphereCollider>();
	    deadTrigger.enabled = false;
	    baseReviveChance = reviveChance;
	    agent = GetComponent<NavMeshAgent>();
    }

    public virtual void Start()
    {
	    
	    behaviorAgent = GetComponent<BehaviorGraphAgent>();
	    
	    // Check if BB reference exists and set them to our variable
	    if (behaviorAgent.BlackboardReference.GetVariable("EnemyBehaviourStates", out bbCurrentState)) {}
	    if (behaviorAgent.BlackboardReference.GetVariable("InvestigatePosition", out bbInvestigatePosition)) {}
	    if (behaviorAgent.BlackboardReference.GetVariable("Target", out bbTarget)) {}
	    if (behaviorAgent.BlackboardReference.GetVariable("ForceChasePlayer", out bbForceChasePlayer)) {}
	    if (behaviorAgent.BlackboardReference.GetVariable("IsTargetDetected", out bbIsTargetDetected)) {}
    }


    private void Update()
    {
	    if (testZombieSight)
	    {
		    if (bbIsTargetDetected.Value)
		    {
			    foreach (GameObject wall in wallsToChange)
			    {
				    wall.GetComponent<MeshRenderer>().material = seenMaterial;
			    }
		    }
		    else
		    {
			    foreach (GameObject wall in wallsToChange)
			    {
				    wall.GetComponent<MeshRenderer>().material = unseenMaterial;
			    }
		    }
	    }

	    if (agent.hasPath && agent.velocity.magnitude < 0.01f)
	    {
		    stuckTimer += Time.deltaTime;
		    if (stuckTimer > maxStuckTime)
		    {
			    agent.ResetPath();
			    stuckTimer = 0;
			    bbCurrentState.Value = EnemyBehaviourStates.ReturnToSpawn;
		    }
	    }
	    else
	    {
		    stuckTimer = 0;
	    }
    }


    //* ===== IDamageable and attack Related =====
    public void TakeDamage(float damageAmount, GameObject damageSource, bool ignoreCooldown)
    {
	    if (isDead) return;
	    if (Time.time <= nextDamageAcceptTime) return;
	    
	    nextDamageAcceptTime = Time.time + damageCooldown;
	    
	    currentHealth -= damageAmount;
	    
	    if (damageSource.TryGetComponent(out PlayerController player) && (int)damageAmount == (int)player.stabDamage)
	    {
		    player.DamageReceivedByTarget(true);
	    }

	    if (currentHealth <= 0)
	    {
		    Death();
		    return;
	    }
	    
	    if (damageSource.CompareTag("Player") && bbCurrentState != EnemyBehaviourStates.Stagger)
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
	    
	    if (damageSource.GetComponent<PlayerController>() != null && (int)damageAmount == (int)player.stabDamage)
	    {
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

    public virtual void OnAttackCollision(Collider otherCollider, bool isRadialAttack)
    {
	    if (otherCollider.TryGetComponent(out IDamageable damageable))
	    {
		    damageable.TakeDamage(hitDamage, gameObject);
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

	    if (bbCurrentState == EnemyBehaviourStates.Stagger) return;
	    
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
	    if (other.gameObject.CompareTag("Player") && !isDead && bbCurrentState != EnemyBehaviourStates.Attack && bbCurrentState != EnemyBehaviourStates.Stagger)
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
