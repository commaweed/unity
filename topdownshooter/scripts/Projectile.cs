using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

   // which layers do we want to hit
   public LayerMask collisonMask;

   [SerializeField] private float speed = 10f;
   public float Speed { set { this.speed = value; } }

   private float damage = 1f;

   private float lifeTime = 3f;

   // need to compensate for when enemy is moving and intersects with bullet, so bullet starts inside collider
   private float skinWidth = 0.1f;

	// Use this for initialization
	void Start () {
      Destroy(gameObject, lifeTime);

      
	}

   private void DetermineIfInsideOtherColliders() {
      // our projectile is intersecting with all of the following colliders
      Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, collisonMask);
      if (initialCollisions.Length > 0) {
         OnHitObject(initialCollisions[0], transform.position);
      }
   }
	
	// Update is called once per frame
	void Update () {
      MoveForward();
   }

   private void CheckCollisions(float moveDistance) {
      // shoot a ray from the current projectile position in a forward direction
      Ray ray = new Ray(transform.position, transform.forward);

      // see what was hit (we want to know which trigger colliders we hit)
      RaycastHit hit;
      // adding skinwidth will handle case where enemies are moving really fast
      if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisonMask, QueryTriggerInteraction.Collide)) {
         OnHitObject(hit.collider, hit.point);
      }
   }

   private void OnHitObject(Collider collider, Vector3 hitPoint) {
      IDamageable damageable = collider.GetComponent<IDamageable>();
      if (damageable != null) {
         damageable.TakeHit(damage, hitPoint, transform.forward);
      }

      GameObject.Destroy(gameObject);
   }

   private void MoveForward() {
      float moveDistance = speed * Time.deltaTime;
      CheckCollisions(moveDistance);
      transform.Translate(Vector3.forward * moveDistance);
   }
}
