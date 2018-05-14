using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Attach this script to the left, right, and mouth collider components (make sure "Is Trigger" is clicked.
/// It represents the script that handles AI damage to the player.
/// </summary>
public class AiDamageTrigger : MonoBehaviour {

   // represents the Animator parameter name that we want to read values from (e.g. right_hand, left_hand, mouth)
   // we configured curves in the animations themselves and set the curve value to 1 whenever the animation was at the correct point
   // this does require that each of the triggers that have this script as a component set it to the name of the parameter
   // NOTE: lower case parameter names were used because mixed case often leads to errors (as it did for Gary in the video) - wish they were like java enums
   [SerializeField] private string parameter;

   [SerializeField] private int bloodParticlesBurstAmount = 10;

   // our fast hash lookup in the Animator
   private int parameterHash = -1;

   private AiStateMachine stateMachine;
   private Animator animator;
   private ParticleSystem bloodParticleSystem;

   /// <summary>
   /// Called once in MonoBehavior life-cycle, just after awake.
   /// </summary>
   private void Start() {
      Initialize();
   }

   /// <summary>
   /// Initializes and validates all the required components, etc.
   /// Note: since missing components, etc. at this point is a problem with configuration, I'm using asserts; 
   /// they'll let me know I forgot to do soemething.  Thus, the null checks shouldn't be necessary.
   /// </summary>
   private void Initialize() {
      Assert.IsNotNull(parameter, "parameter missing; Did you forget to set the parameter value to an Animator parameter name in this script?");

      // root = the top-level parent; in this case it is "Omni Zombie Jill"
      stateMachine = transform.root.GetComponentInChildren<AiStateMachine>();
      Assert.IsNotNull(stateMachine, "Missing AiStateMachine in an ancestor component of AiDamageTrigger!");
      if (stateMachine != null) {
         animator = stateMachine.Animator;
         Assert.IsNotNull(animator, "Missing Animator in the AiStateMachine; did you forget to add the AnimatorController in the inspector?");
         parameterHash = Animator.StringToHash(parameter);
      }

      Assert.IsNotNull(
         GameSceneManager.Instance,
         "GameSceneManager.Instance is missing; did you add it to the scene?"
      );
      this.bloodParticleSystem = GameSceneManager.Instance.BloodParticles;

      Assert.IsNotNull(Camera.main, "Missing Main Camera; is there one in the scene with Tag MainCamer?");
   }

   /// <summary>
   /// Indicates the AI Entity is causing damage.  This will be true when the animation curve is returning 1.
   /// </summary>
   /// <returns>true if the zombie is causing damage.</returns>
   private bool IsCausingDamage() {
      return animator.GetFloat(parameter) > 0.9f;
   }

   /// <summary>
   /// Displays the blood particle effect.
   /// </summary>
   private void DisplayBloodParticles() {
      bloodParticleSystem.transform.position = transform.position; // the position of the component this script is on
      bloodParticleSystem.transform.rotation = Camera.main.transform.rotation;
      var main = bloodParticleSystem.main;
      main.simulationSpace = ParticleSystemSimulationSpace.World;
      bloodParticleSystem.Emit(this.bloodParticlesBurstAmount);
   }

   private void OnTriggerStay(Collider other) {
      if (other.gameObject.CompareTag("Player") && IsCausingDamage()) {
         DisplayBloodParticles();
      }
   }
}
