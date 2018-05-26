using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

using Rpg.Weapon;
using Rpg.CameraUi;

namespace Rpg.Character {
   public class Player : MonoBehaviour, IDamageable {

      [SerializeField]
      private float maxHealthPoints = 100f;

      [SerializeField]
      private float damagePerHit = 50f;

      [SerializeField]
      private MyWeapon weaponInUse;

      [SerializeField]
      AudioClip[] tookDamageSounds;

      [SerializeField]
      AudioClip[] giveDamageSounds;

      [SerializeField]
      AudioClip deathSound;

      [SerializeField]
      AnimatorOverrideController animatorOverrideController;

      private GameObject currentTarget;

      private bool isDead = false;
      public bool IsDead { get { return this.isDead; } }

      private float currentHealthPoints;
      public float HealthPercentage {
         get { return this.currentHealthPoints / this.maxHealthPoints; }
      }

      private CameraRaycaster rayCaster;

      Animator animator;
      AudioSource audioSource;

      private float lastHitTime = 0f;

      private void Start() {
         ValidateMainCameraExistence();

         InitializePlayer();
         InitializeAnimator();
         InitializeAudioSource();

         RegisterMouseClick();

         PutWeaponInHand();
      }

      private void InitializePlayer() {
         this.currentHealthPoints = maxHealthPoints;
      }
       
      private void InitializeAnimator() {
         animator = GetComponent<Animator>();
         this.animator.runtimeAnimatorController = this.animatorOverrideController;
      }

      private void InitializeAudioSource() {
         audioSource = GetComponent<AudioSource>();
      }

      private void PutWeaponInHand() {
         var weaponPrefab = weaponInUse.GetWeaponPrefab();
         GameObject dominantHand = RequestDominantHand();
         var weapon = Instantiate(weaponPrefab, dominantHand.transform);
         weapon.transform.localPosition = weaponInUse.GripTransform.localPosition;
         weapon.transform.localRotation = weaponInUse.GripTransform.localRotation;
      }

      private GameObject RequestDominantHand() {
         Component[] dominantHand = GetComponentsInChildren<DominantHand>();
         Assert.AreNotEqual(dominantHand.Length, 0, "No DominantHand found on player.");
         Assert.IsFalse(dominantHand.Length > 1, "Too many dominant hands [" + dominantHand.Length + "]; allowed = 1");
         return dominantHand[0].gameObject;
      }

      private void ValidateMainCameraExistence() {
         if (Camera.main == null) {
            Debug.LogWarning(
                "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\"," +
                " for camera-relative controls.", gameObject);
         }
      }

      private void RegisterMouseClick() {
         this.rayCaster = Camera.main.GetComponent<CameraRaycaster>();

         // register with the CameraRaycaster that we are listening to layer hit changes
         this.rayCaster.notifyMouseClickObservers += OnMouseClick;
      }

      /// <summary>
      /// Event handler that fires whenever the CameraRaycaster layer hit changes.
      /// </summary>
      private void OnMouseClick(RaycastHit itemHit, int layerHit) {
         if (!this.isDead && layerHit == (int) Layer.Enemy) {
            GameObject target = itemHit.collider.gameObject;
            if (IsTargetInRange(target)) {
               Enemy enemy = target.GetComponent<Enemy>(); // this might not work
               HandleEnemyAttack(enemy);
            }
         }
      }

      private void HandleEnemyAttack(Enemy enemy) {
         if (Time.time - lastHitTime > this.weaponInUse.MinSecondsBetweenHits) {
            this.transform.LookAt(enemy.transform);
            PlayAudioClip(AudioClipType.DEAL_DAMAGE);
            AnimateAttack();
            enemy.TakeDamage(damagePerHit);
            lastHitTime = Time.time;
         }
      }

      private void AnimateAttack() {
         this.animatorOverrideController["DEFAULT_ATTACK"] = this.weaponInUse.GetAttackAnimation();
         animator.SetBool("Attack", true);
      }

      private enum AudioClipType { DEAL_DAMAGE, TAKE_DAMAGE, DIE }
      private void PlayAudioClip(AudioClipType type) {
         switch (type) {
            case AudioClipType.DEAL_DAMAGE:
               audioSource.clip = giveDamageSounds[UnityEngine.Random.Range(0, giveDamageSounds.Length)];
               break;
            case AudioClipType.TAKE_DAMAGE:
               audioSource.clip = tookDamageSounds[UnityEngine.Random.Range(0, tookDamageSounds.Length)];
               break;
            case AudioClipType.DIE:
               audioSource.clip = this.deathSound;
               break;
         }

         if (!this.audioSource.isPlaying) this.audioSource.Play();
      }

      private bool IsTargetInRange(GameObject target) {
         float distance = Vector3.Distance(target.transform.position, this.transform.position);
         return distance <= this.weaponInUse.AttackRadius;
      }

      public void TakeDamage(float amount) {
         currentHealthPoints = Mathf.Clamp(currentHealthPoints - amount, 0, maxHealthPoints);
         if (currentHealthPoints == 0) {
            StartCoroutine(KillPlayer());
         } else {
            PlayAudioClip(AudioClipType.TAKE_DAMAGE);
         }
      }

      IEnumerator KillPlayer() {
         isDead = true;
         PlayAudioClip(AudioClipType.DIE);
         animator.SetBool("Death", true);
         iTween.ColorTo(this.gameObject, iTween.Hash( // TODO: this is also inthe enemy class
            "color", new Color(0, 0, 0, 0),
            "time", 3f,
            "delay", 1f,
            "easetype", iTween.EaseType.linear,
            "includechildren", true,
            "onComplete", "ReloadScene"
         ));
         yield return null;
      }

      private void ReloadScene() {
         UnityEngine.SceneManagement.SceneManager.LoadScene(0);
      }

   }
}
