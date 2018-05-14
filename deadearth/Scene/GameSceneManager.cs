using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Acts like a database for the AiStateMachine.
/// </summary>
public class GameSceneManager : MonoBehaviour {

   #region Singleton
   // this works even when not in scene and is never appears to be null afterwards
   public static GameSceneManager Instance;

   /// <summary>
   /// Initialize at the beginning of the life cycle.  Instantiate the GameSceneManager one time such that it follows the
   /// Singleton pattern.
   /// </summary>
   public void Awake() {
      /* checking for null should only be necessary if you add more than one GameSceneManager to the same scene, otherwise,
       * the last one will take precedence.
       */ 
      if (Instance == null) {
         Instance = this;
         Assert.IsNotNull(
            bloodParticles,
            "bloodParticles is missing; did you forget to drag it to the inspector on the GameSceneManager?"
         );
      }
   }
   #endregion

   /*
    1.  Drop blood texture with transparent background
    2.  set texture's "Alpha is Transparency" checkbox to true - alpha channel is to be used
    3.  Create a material "Blood Particles"
    4.  Change the shader to Particles - Alpha Blended - where transparent, nothing is rendered, where opaque, rendered
    5.  Drag image to Texture box
    6.  Click the color tint button above that and change to dark red
    7.  Then go to inspector
    8.  Go to bottom and under render add material (click on material picker)
    9.  We will eventually disable "Emission", for now leave on so we can see it
    10. Rotate particle system so cone is facing from mouth to jill's back (-180x) - eventually we'll add a mount
    11. Make base of cone small - Shape.Radius = 0.02, Shape.Angle = 20
    12. goto top in main section and change:
         Start lifetime - how long they live - use random between two constants - 0.3, 3 (random in seconds)
         Start Speed - random between two constants - 0.3, 0.8
         Start Size - random betwwen two constants - 0.09, 0.29
         Start Rotation - random between two constants - 0, 180 degrees
         Start Color - random between two colors - white and dark red
         Gravity modifier - 0.1 (fall towards ground)
         Simulation Space - World - the particles animate in world space and not the emitter local space
     13. goto Color over liftime - only change the top one = alpha (left = start of lifetime, right =end of lifetime)
         the bottom is the color (can set at start and at end) - leave this one alone
            leave left alpha=255, but click on right and set alpha=0, then drag right to location=78% (they become more transparent as they end)
     14. goto rotation over lifetime and check it - random between two constants, 0,90
     15. now disable emissions - we are going to emit via code
    */
   [SerializeField] private ParticleSystem bloodParticles;

   /// <summary>
   /// The cache of all the collider to their corresponding state machine for fast lookup.  The key represents a
   /// collider ID (i.e. GetInstanceID()) and the value is the reference to the AiStateMachine that belongs to it.
   /// </summary>
   private Dictionary<int, AiStateMachine> cache = new Dictionary<int, AiStateMachine>();

   /// <summary>
   /// Store the given stateMachine in the cache according to the provided key.  If it is already in the cache,
   /// nothing will be stored.
   /// </summary>
   /// <param name="collider">The collider to use as the cache key.</param>
   /// <param name="stateMachine">The AiStateMachine to cache.</param>
   public void RegisterStateMachine(Collider collider, AiStateMachine stateMachine) {
      if (collider != null && !cache.ContainsKey(collider.GetInstanceID())) {
         this.cache[collider.GetInstanceID()] = stateMachine;
      }
   }

   /// <summary>
   /// Return the cached item represented by the given collider or null if it could not be found.
   /// </summary>
   /// <param name="collider">The collider to use in the lookup.</param>
   /// <returns>The cached AiStateMachine or null if it could not be found.</returns>
   public AiStateMachine GetStateMachine(Collider collider) {
      return (collider == null) ? null : this.cache[collider.GetInstanceID()];
   }

   /// <summary>
   /// Represents a reference to the blood particles system that is in the scene.  It will be reused for each of the 
   /// zombie biting blood effects, such as when the feed on dead bodies.
   /// </summary>
   public ParticleSystem BloodParticles { get { return this.bloodParticles; } }
}
