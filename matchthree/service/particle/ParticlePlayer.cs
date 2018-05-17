using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ParticlePlayer : MonoBehaviour {

   private ParticleSystem[] particles;
   private const float lifetime = 1f;

   // Use this for initialization
   void Start () {
      this.particles = GetComponentsInChildren<ParticleSystem>();
      Assert.IsNotNull(particles, "Missing ParticleSystem[] as child components; did you add this script to the empty particle Fx object?");
      Destroy(gameObject, lifetime);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

   public void Play() {
      if (particles != null) {
         foreach (ParticleSystem ps in particles) {
            ps.Stop();
            ps.Play();
         }
      }
   }
}
