using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Rpg.Weapon;

namespace Rpg.Character {
   public class Knight : Enemy {

      // TODO: separate out character firing logic into a separate class
      override protected IEnumerator PerformAttack() {
         this.transform.LookAt(attackTarget);
         PlayAudioClip(AudioClipType.DEAL_DAMAGE);
         AnimateAttack();
         DoDamage(this.attackTarget.gameObject);
         yield return null;
      }

      private void DoDamage(GameObject other) {
         IDamageable damageableComponent = (IDamageable) other.GetComponent(typeof(IDamageable));
         if (damageableComponent != null) {
            damageableComponent.TakeDamage(this.damagePerHit);
         } 
      }

   }
}
