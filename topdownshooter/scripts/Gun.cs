using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FireMode { Auto, Burst, Single }

public class Gun : MonoBehaviour {

   // need to know position of muzzle so we can instantiate projectiles from that position
   [Header("Prefabs")]
   [SerializeField] private Transform[] projectileSpawnLocations;
   [SerializeField] private Projectile projectile;
   [SerializeField] private Transform shellCasing;
   [SerializeField] private Transform shellEjectionPoint;

   [Header("Effects")]
   [SerializeField] private FireMode fireMode;
   [SerializeField] private float rateOfFire = 100f; // milliseconds between shots
   [SerializeField] private float muzzleVelocity = 35f; // speed at which the bullet will leave the gun
   [SerializeField] private int burstCount = 3;
   [SerializeField] private int magazineSize = 20;
   [SerializeField] private float reloadTime = 0.3f;

   [Header("Recoil")]
   [SerializeField] private Vector2 recoilKickbackMinMax = new Vector2(0.05f, 0.2f);
   [SerializeField] private Vector2 recoilAngleMinMax = new Vector2(3f, 5f);
   [SerializeField] private float recoilMoveSettleTime = 0.1f;
   [SerializeField] private float recoilRotationSettleTime = 0.1f;

   private float nextShotTime;
   private MuzzleFlash muzzleFlash;
   private bool triggerReleasedSinceLastShot;
   private int shotRemainingInBurst;
   private int projectilesRemainingInMagazine;
   private bool isReloading;

   private Vector3 recoilSmoothDampVelocity;
   private float recoilRotationSmoothDampVelocity;
   private float recoilAngle;

   private void Start() {
      this.muzzleFlash = GetComponent<MuzzleFlash>();
      this.shotRemainingInBurst = this.burstCount;
      this.projectilesRemainingInMagazine = this.magazineSize;
   }

   // LateUpdate because we want this to happen after we do the Aim
   private void LateUpdate() {
      HandleGunRecoil();

      if (!isReloading && projectilesRemainingInMagazine == 0) {
         Reload();
      }
   }

   private void HandleGunRecoil() {
      // animate forward and back recoil 
      transform.localPosition = Vector3.SmoothDamp(
         transform.localPosition,            // current position
         Vector3.zero,                       // target position
         ref recoilSmoothDampVelocity,       // not set by us; just a reference
         recoilMoveSettleTime                // how long for it to return to target position
      );

      // now set the local rotation
      recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotationSmoothDampVelocity, recoilRotationSettleTime);
      transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;
}

   void Shoot() {
      if (!isReloading && Time.time > nextShotTime && this.projectilesRemainingInMagazine > 0) {

         if (fireMode == FireMode.Burst) {
            if (shotRemainingInBurst == 0) {
               return;
            }
            shotRemainingInBurst--;
         } else if (fireMode == FireMode.Single) {
            if (!triggerReleasedSinceLastShot) {
               return;
            }
         }

         for (int i = 0; i < projectileSpawnLocations.Length; i++) {
            if (projectilesRemainingInMagazine == 0) {
               break;
            }
            projectilesRemainingInMagazine--;
            UpdateShotTimer();
            Projectile newProjectile = Instantiate(
               projectile, 
               projectileSpawnLocations[i].position, 
               projectileSpawnLocations[i].rotation
            ) as Projectile;
            newProjectile.Speed = muzzleVelocity;
         }

         // expel the shell casings
         Instantiate(shellCasing, shellEjectionPoint.position, shellEjectionPoint.rotation);

         // display the muzzle flash
         muzzleFlash.Activate();

         // recoil on the weapon (use random value for kickback range)
         transform.localPosition -= Vector3.forward * Random.Range(recoilKickbackMinMax.x, recoilKickbackMinMax.y);
         recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
         recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);
      }
   }

   public void Reload() {
      if (!isReloading && projectilesRemainingInMagazine != magazineSize) {
         StartCoroutine(AnimateReloadRoutine());
      }
   }

   private IEnumerator AnimateReloadRoutine() {
      isReloading = true;
      yield return new WaitForSeconds(0.2f);

      float reloadSpeed = 1f / reloadTime;
      float percent = 0; // how far into the animation we are

      Vector3 initialRotation = transform.localEulerAngles;
      float maxReloadAngle = 30;

      while (percent < 1) {
         percent += Time.deltaTime * reloadSpeed;

         // rotate gun up and down using the parabola
         // need to go to 0 then 1 then back to 0, so use parabola (y=4(-x^2+x))
         float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
         float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
         transform.localEulerAngles = initialRotation + Vector3.left * reloadAngle;

         yield return null;
      }

      isReloading = false;
      projectilesRemainingInMagazine = magazineSize;
   }

   private void UpdateShotTimer() {
      this.nextShotTime = Time.time + rateOfFire / 1000f;
   }

   public void OnTriggerHold() {
      Shoot();
      triggerReleasedSinceLastShot = false;
   }

   public void OnTriggerRelease() {
      triggerReleasedSinceLastShot = true;
      shotRemainingInBurst = burstCount;
   }

   public void Aim(Vector3 aimPoint) {
      if (!isReloading) {
         transform.LookAt(aimPoint);
      }
   }
}
