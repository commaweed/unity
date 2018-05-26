using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityEngine.AI;

using Rpg.Weapon;

namespace Rpg.Character {

   [RequireComponent(typeof(AICharacterControl))]
   [RequireComponent(typeof(NavMeshAgent))]
   //[RequireComponent(typeof(ThirdPersonCharacter))]
   public abstract class Enemy : MonoBehaviour, IDamageable {

      [SerializeField]
      protected float maxHealthPoints = 100f;
      protected float currentHealthPoints;

      [SerializeField]
      protected float damagePerHit = 4f;

      [SerializeField]
      protected float secondsBetweenShots = .5f;

      [SerializeField]
      protected float attackRadius = 5f;

      [SerializeField]
      protected float chaseRadius = 10f;

      protected float lastShotTime = 0f;

      protected Transform attackTarget;

      protected Animator animator;
      protected AudioSource audioSource;

      [SerializeField]
      AnimatorOverrideController animatorOverrideController;

      [SerializeField]
      protected AudioClip[] deathSounds;

      [SerializeField]
      protected AudioClip attackSound;

      [SerializeField]
      protected AudioClip takeDamageSound;

      [SerializeField]
      AnimationClip attackAnimation;

      // sybling components
      //private ThirdPersonCharacter thirdPersonCharacter;
      protected AICharacterControl aiController;

      protected bool isDead = false;

      // Use this for initialization
      void Start() {
         //this.thirdPersonCharacter = GetComponent<ThirdPersonCharacter>();
         this.aiController = GetComponent<AICharacterControl>();
         InitializeAnimator();
         audioSource = GetComponent<AudioSource>();

         this.attackTarget = aiController.target;
         if (this.attackTarget == null) {
            throw new System.ArgumentException("target is missing; Did you forget to add the target to the AiController script?");
         }

         currentHealthPoints = this.maxHealthPoints;
      }

      private void InitializeAnimator() {
         animator = GetComponent<Animator>();
         this.animator.runtimeAnimatorController = this.animatorOverrideController;

         // remove events to prevent 'hit' error
         this.attackAnimation.events = new AnimationEvent[0];
      }

      // Update is called once per frame
      void Update() {
         if (this.isDead) {
            aiController.SetTarget(this.transform);
            return;
         }

         float distance = Vector3.Distance(attackTarget.position, this.transform.position);
         HandleEnemyCharge(distance);
         HandleEnemyAttack(distance);
      }

      private void HandleEnemyCharge(float distance) {
         if (distance <= this.chaseRadius) {
            aiController.SetTarget(attackTarget);
         } else {
            aiController.SetTarget(this.transform);
         }
      }

      private void HandleEnemyAttack(float distance) {
         Player player = attackTarget.GetComponent<Player>();
         if (player != null && !player.IsDead) {
            if (distance <= this.attackRadius && Time.time - lastShotTime > secondsBetweenShots) {
               StartCoroutine(PerformAttack());
               lastShotTime = Time.time;
            }
         }
      }

      public void TakeDamage(float amount) {
         currentHealthPoints = Mathf.Clamp(currentHealthPoints - amount, 0, maxHealthPoints);
         if (currentHealthPoints == 0) {
            this.isDead = true;
            StartCoroutine(KillEnemy());
         }
      }

      protected virtual IEnumerator KillEnemy() {
         animator.SetBool("Death", true);

         PlayAudioClip(AudioClipType.DIE);

         this.gameObject.GetComponent<Collider>().enabled = false;
         iTween.ColorTo(this.gameObject, iTween.Hash(
            "color", new Color(0, 0, 0, 0),
            "time", 3f,
            "delay", 1f,
            "easetype", iTween.EaseType.linear,
            "includechildren", true,
            "onComplete", "DestroyEnemy"
         ));

         yield return null;
      }

      private void DestroyEnemy() {
         Destroy(this.gameObject);
      }

      protected enum AudioClipType { DEAL_DAMAGE, TAKE_DAMAGE, DIE }
      protected void PlayAudioClip(AudioClipType type) {
         switch (type) {
            case AudioClipType.DEAL_DAMAGE:
               audioSource.clip = attackSound;
               break;
            case AudioClipType.TAKE_DAMAGE:
               audioSource.clip = takeDamageSound;
               break;
            case AudioClipType.DIE:
               audioSource.clip = deathSounds[UnityEngine.Random.Range(0, deathSounds.Length)];
               break;
         }

         if (audioSource.clip != null && !this.audioSource.isPlaying) this.audioSource.Play();
      }

      protected void AnimateAttack() {
         this.animatorOverrideController["DEFAULT_ATTACK"] = this.attackAnimation;
         animator.SetBool("Attack", true);
      }

      public float HealthPercentage {
         get { return this.currentHealthPoints / this.maxHealthPoints; }
      }
      protected abstract IEnumerator PerformAttack();

      /// <summary>
      /// Callback that fires whenever the Gizmos button is pressed.  It will provide some visuals to help us determine
      /// if our move to mouse click is working.
      /// </summary>
      private void OnDrawGizmos() {
         Gizmos.color = Color.black;
         Gizmos.DrawWireSphere(transform.position, this.attackRadius);

         Gizmos.color = Color.red;
         Gizmos.DrawWireSphere(transform.position, this.chaseRadius);
      }
   }
}
