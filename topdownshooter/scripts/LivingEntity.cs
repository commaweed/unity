using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable {

   [SerializeField] private float startingHealth;
   public float StartingHealth { get { return this.startingHealth; } set { this.startingHealth = value; } }

   protected float health;
   protected bool dead;

   // create an event to be fired when a livingentity dies
   public event System.Action OnDeath;

   // Use this for initialization
   protected virtual void Start () {
      this.health = startingHealth;
	}

   public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection) {
      // do something with itemThatWasHit related to particle effect
      TakeDamage(damage);
   }

   public virtual void TakeDamage(float damage) {
      health -= damage;
      if (!dead && health <= 0) {
         Die();
      }
   }

   [ContextMenu("Self Destruct")]
   protected void Die() {
      dead = true;
      if (OnDeath != null) {
         OnDeath(); // fire the event
      }
      GameObject.Destroy(gameObject);
   }
}
