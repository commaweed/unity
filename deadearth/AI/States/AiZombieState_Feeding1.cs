using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class AiZombieState_Feeding1 : AiZombieState {

   // want to smooth rotation when spinning head facing direction with food target
   [SerializeField] private float slerpSpeed = 5.0f;

   // a script on a prefab can reference other gameobjects that are within its own hierarchy
   // because they too are scene independent
   [SerializeField] private Transform bloodParticlesMount;

   // how long to burst
   [SerializeField] [Range(0.01f, 1.0f)] private float bloodParticlesBurstTime = 0.1f;

   // how many particles to burst
   [SerializeField] [Range(1, 100)] private int bloodParticlesBurstAmount = 10;

   private float timer;
   private ParticleSystem bloodParticleSystem;

   // should be same for all eating entities
   // this is the name we gave the zombie_eating animimation it in the Animator
   private int eatingStateHash = Animator.StringToHash("Feeding State");

   // the cinematic layer
   private int eatingLayerIndex = -1;

   protected override void Awake() {
      base.Awake();
      Assert.IsNotNull(
         bloodParticlesMount, 
         "bloodParticlesMount is missing in feeding script; did you forget to drag it to the inspector on an AiEntity?"
      );
   }

   protected override void Start() {
      base.Start();

      Assert.IsNotNull(
         GameSceneManager.Instance,
         "GameSceneManager.Instance is missing; did you add it to the scene?"
      );

      this.bloodParticleSystem = GameSceneManager.Instance.BloodParticles; // we already check to ensure 
   }

   /// <summary>
   /// Returns the default state.
   /// </summary>
   /// <returns>Feeding</returns>
   public override AiStateType GetDefaultStateType() {
      return AiStateType.Feeding;
   }

   /// <summary>
   /// Callback that is fired when this state first becomes active.
   /// </summary>
   public override void OnEnterState() {
      base.OnEnterState();

      // Get layer index
      if (this.eatingLayerIndex == -1) {
         this.eatingLayerIndex = zombieStateMachine.Animator.GetLayerIndex("Cinematic");
      }

      // Configure the State Machine's Animator
      zombieStateMachine.Feeding = true;
      zombieStateMachine.Seeking = 0;
      zombieStateMachine.Speed = 0;
      zombieStateMachine.AttackType = 0;

      // Agent updates postion but not rotation (feeding animation should be done by animations)
      zombieStateMachine.ModifyNavAgentUpdateAttributes(true, false);

      ResetTimer();
   }

   /// <summary>
   /// Callback that fires then this state exits.
   /// </summary>
   public override void OnExitState() {
      zombieStateMachine.Feeding = false;
   }

   /// <summary>
   /// Called by the state machine each frame.
   /// </summary>
   /// <returns>Either Idle or a new state based upon the threats that were processed</returns>
   public override AiStateType OnUpdate() {
      IncrementTimer();

      AiStateType state = GetDefaultStateType();

      // if we got to this state, but its not hungry, go back to alert state
      if (!zombieStateMachine.IsHungery()) {
         zombieStateMachine.WaypointManager.TrackWayPoint();
         state = AiStateType.Alerted; 
      } else {
         AiThreatManager manager = zombieStateMachine.ThreatManager;
         
         if (manager.DoesPlayerThreatExist() || manager.DoesLightThreatExist()) {
            manager.TrackTarget(zombieStateMachine.ThreatManager.CurrentVisualThreat);
            state = AiStateType.Alerted;
         } else if (manager.DoesAudioThreatExist()) {
            manager.TrackTarget(zombieStateMachine.ThreatManager.CurrentAudioThreat);
            state = AiStateType.Alerted;
         } else if (manager.IsTargeting(AiTargetType.Visual_Food)) {
            if (IsZombieCurrentlyEating()) {
               ReplenishSatisfaction();
            }
         }

         FaceTargetGradually(this.slerpSpeed);
      }

      return state;
   }

   /// <summary>
   /// Similutes eating so that the zombie can fill up and be satisfied :).
   /// </summary>
   private void ReplenishSatisfaction() {
      // satisfy zombie
      zombieStateMachine.Satisfaction = Mathf.Min(
         zombieStateMachine.Satisfaction + ((Time.deltaTime * zombieStateMachine.ReplenishRate) / 100.0f),
         1.0f
      );

      DisplayBloodParticles();
   }

   /// <summary>
   /// Displays the blood particle effect that accompanies replenishing of satisfaction.
   /// </summary>
   private void DisplayBloodParticles() {
      if (HasReachedMaxTime()) {
         bloodParticleSystem.transform.position = this.bloodParticlesMount.transform.position;
         bloodParticleSystem.transform.rotation = bloodParticlesMount.transform.rotation;
         var main = bloodParticleSystem.main;
         main.simulationSpace = ParticleSystemSimulationSpace.World;
         bloodParticleSystem.Emit(this.bloodParticlesBurstAmount);
         ResetTimer();
      }
   }

   /// <summary>
   /// Indicates whether or not the the feeding animation is currently playing (that's the middle one).  
   /// If it is, it means the zombie is currently eating.
   /// </summary>
   /// <returns>true if the zombie is currently eating</returns>
   private bool IsZombieCurrentlyEating() {
      return zombieStateMachine.Animator
         .GetCurrentAnimatorStateInfo(eatingLayerIndex)
         .shortNameHash == eatingStateHash;
   }


   private void IncrementTimer() {
      timer += Time.deltaTime;
   }

   private bool HasReachedMaxTime() {
      return timer > bloodParticlesBurstTime;
   }

   private void ResetTimer() {
      timer = 0.0f;
   }
}
