using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rpg.Weapon {
   public class Projectile : MonoBehaviour {

      [SerializeField] private float speed = 10f;
      public float Speed { get { return this.speed; } }

      private GameObject shooter;
      public GameObject Shooter {
         get { return this.shooter; }
         set { this.shooter = value; }
      }

      public float DamageAmount { get; set; }

      private void OnCollisionEnter(Collision other) {
         DoDamage(other.gameObject);
      }

      private void OnTriggerEnter(Collider other) {
         DoDamage(other.gameObject);
      }

      private void DoDamage(GameObject other) {
         if (this.shooter && other.layer != this.shooter.layer) {
            IDamageable damageableComponent = (IDamageable) other.GetComponent(typeof(IDamageable));
            if (damageableComponent != null) {
               damageableComponent.TakeDamage(this.DamageAmount);
               Destroy(this.gameObject); // hit
            } else {
               Destroy(this.gameObject, 3f); // miss
            }
         }
      }
   }
}
