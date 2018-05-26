using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity {

   [SerializeField] private ParticleSystem deathFx;

   public enum State { Idle, Chasing, Attacking }
   private State currentState;

   private NavMeshAgent pathFinder;
   private Transform target;
   private LivingEntity targetEntity;
   private Material skinMaterial;

   private Color originalColor;

   private float attackDistanceThreshold = 1.5f;
   private float timeBetweenAttacks = 1f;
   private float damage = 1;

   private float nextAttackTime;

   // we don't want the enemy to get onto of the player
   private float myCollisionRadius;
   private float targetCollisionRadius;

   private bool hasTarget;


   private void Awake() {
      InializeComponents();
   }

   // Use this for initialization
   protected override void Start () {
      base.Start();
      InitializeState();
      
   }
	
	// Update is called once per frame
	void Update () {
      HandleAttack();
   }

   private void InializeComponents() {
      this.pathFinder = GetComponent<NavMeshAgent>();
      GameObject player = GameObject.FindGameObjectWithTag("Player");
      if (player != null) {
         this.hasTarget = true;
         this.target = player.transform;
         this.targetEntity = target.GetComponent<LivingEntity>();

         this.myCollisionRadius = GetComponent<CapsuleCollider>().radius;
         this.targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
      }
   }

   private void InitializeState() {
      if (hasTarget) {
         this.currentState = State.Chasing;
         targetEntity.OnDeath += OnTargetDeath;
         StartCoroutine(UpdateAgentPathRoutine());
      }
   }

   public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection) {
      if (damage >= health) {
         // die in explosion
         Destroy(
            Instantiate(deathFx.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)) as GameObject,
            deathFx.main.startLifetimeMultiplier
         );
      }

      StartCoroutine(TakeDamageAnimationRoutine());
      base.TakeHit(damage, hitPoint, hitDirection);
   }

   IEnumerator TakeDamageAnimationRoutine() {
      skinMaterial.color = Color.yellow;
      yield return new WaitForSeconds(0.2f);
      skinMaterial.color = originalColor;
   }

   // OnDeath event callback
   private void OnTargetDeath() {
      hasTarget = false;
      this.currentState = State.Idle;
   }
   
   private void HandleAttack() {
      if (hasTarget) {
         if (Time.time > nextAttackTime) {

            // Vector3.Distance computes squareroot which can be expensive, so...
            float sqrDistanceToTarget = (target.position - transform.position).sqrMagnitude;

            // attackDistanceThreshold is from edge of the two colliders
            // to get it to the center of the two colliders, we need to add the radius of both colliders to it
            if (sqrDistanceToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2)) {
               nextAttackTime = Time.time + timeBetweenAttacks;
               StartCoroutine(AttackCoroutine());
            }
         }
      }
   }

   private IEnumerator AttackCoroutine() {
      currentState = State.Attacking;
      pathFinder.enabled = false;
     
      Vector3 originalPosition = transform.position;
      Vector3 directionToTarget = (target.position - transform.position).normalized;
      // we don't want the attack to intercept with the player
      Vector3 attackPosition = target.position - directionToTarget * (myCollisionRadius);

      float attackSpeed = 3f;

      // animate our lunge
      skinMaterial.color = Color.red;
      bool hasAppliedDamage = false;
      float percent = 0;
      while (percent <= 1) {

         if (percent >= 0.5f && !hasAppliedDamage) {
            hasAppliedDamage = true;
            this.targetEntity.TakeDamage(damage);
         }

         percent += Time.deltaTime * attackSpeed;

         // need to go to 0 then 1 then back to 0, so use parabola (y=4(-x^2+x))
         float interpolation = (-Mathf.Pow(percent,2) + percent) * 4;

         // move along parabola over time (interpolation = 0 => original position, 1 => attack position)
         transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

         yield return null;
      }

      skinMaterial.color = originalColor;
      currentState = State.Chasing;
      pathFinder.enabled = true;
   }

   /// <summary>
   /// Since SetDestination is expensive due to recalculating path, using coroutine on refresh timer
   /// </summary>
   /// <returns></returns>
   IEnumerator UpdateAgentPathRoutine() {
      float refreshRate = .25f; // calculating 4 times a second rather than at the update frame rate (e.g. 60 times a second)
      while (hasTarget) {
         if (currentState == State.Chasing) {
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            // we want the enemy to stop some distance away from the player (not be on top of him)
            //Vector3 targetPosition = new Vector3(target.position.x, 0, target.position.z);
            Vector3 targetPosition = target.position - directionToTarget
               * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold / 2); 
            if (!dead) {
               pathFinder.SetDestination(targetPosition); // expensive
            }
         }
         yield return new WaitForSeconds(refreshRate);
      }
   }

   /// <summary>
   /// Sets the Enemy characteristics based upon the configured Wave.  It will be called before the Start() method is called.
   /// Thus make sure the NavMeshAgent initialization is done within the Awake() method.
   /// </summary>
   /// <param name="wave">A configuration for a Wave of enemies that is done when setting up the waves in the inspectgor.</param>
   public void SetCharacteristics(Wave wave) {
      pathFinder.speed = wave.MoveSpeed;
      if (hasTarget) {
         damage = Mathf.Ceil(targetEntity.StartingHealth / wave.HitsToKillPlayer);
      }
      StartingHealth = wave.EnemyHealth;

      // changing particle system colors isn't working
      //var mainModule = deathFx.main;
      //mainModule.startColor = new Color(wave.SkinColor.r, wave.SkinColor.g, wave.SkinColor.b, 1);
      //deathFx.startColor = new Color(wave.SkinColor.r, wave.SkinColor.g, wave.SkinColor.b, 1);
      //ParticleSystem.MainModule settings = GetComponent<ParticleSystem>().main;
      //settings.startColor = new ParticleSystem.MinMaxGradient(wave.SkinColor);

      skinMaterial = GetComponent<Renderer>().material;
      skinMaterial.color = wave.SkinColor;
      originalColor = skinMaterial.color;
   }
}
