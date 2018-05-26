using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour {

   private Rigidbody rigidBody;
   private Vector3 velocity;


	// Use this for initialization
	void Start () {
      this.rigidBody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

   // only move rigidbody in physics update; needs to be execute in small regular steps so it never goes through an object
   private void FixedUpdate() {
      rigidBody.MovePosition(rigidBody.position + (velocity * Time.fixedDeltaTime));
   }

   public void Move(Vector3 velocity) {
      this.velocity = velocity;
   }

   public void LookAt(Vector3 lookPoint) {
      // look at the point, however this will cause our player to stoop down; so raise point up on the y-axis to be level with the player
      Vector3 higherLookPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
      transform.LookAt(higherLookPoint);
   }
}
