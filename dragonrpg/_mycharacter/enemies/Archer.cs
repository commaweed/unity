using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Rpg.Weapon;

namespace Rpg.Character {
   public class Archer : Enemy {

      [SerializeField]
      private GameObject projectileToUse;
      [SerializeField]
      private GameObject projectileSocket;

      // TODO: separate out character firing logic into a separate class
      override protected IEnumerator PerformAttack() {
         this.transform.LookAt(attackTarget);
         GameObject newProjectile = Instantiate(
            projectileToUse,
            projectileSocket.transform.position,
            projectileSocket.transform.rotation
         );

         PlayAudioClip(AudioClipType.DEAL_DAMAGE);

         Projectile projectile = newProjectile.GetComponent<Projectile>();
         projectile.DamageAmount = this.damagePerHit;
         projectile.Shooter = this.gameObject;

         // set target up 1 unit (.5 height of charactger would be nice)
         Vector3 target = new Vector3(
            attackTarget.position.x,
            attackTarget.position.y + 1,
            attackTarget.position.z
         );
         Vector3 unitVectorToPlayer = (target - projectileSocket.transform.position).normalized; // direction

         newProjectile.GetComponent<Rigidbody>().velocity = unitVectorToPlayer * projectile.Speed;

         Destroy(newProjectile, 3);

         yield return null;
      }

      // TODO: find a better way to disable the arrows (maybe use a tag)
      private void HideArrows() {
         // this is bad because we could change the name of the prefab 
         Transform[] transforms = this.GetComponentsInChildren<Transform>();
         foreach (Transform t in transforms) {
            if (t.gameObject.name == "Arrows") {
               t.gameObject.SetActive(false);
            }
         }

         // this one is also bad because we could change position
         //var arrows = this.transform.GetChild(0).GetChild(0).gameObject;
      }

      override protected IEnumerator KillEnemy() {
         HideArrows();
         yield return base.KillEnemy();
      }
   }
}
