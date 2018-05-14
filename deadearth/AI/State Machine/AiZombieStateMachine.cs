using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// which direction do we want to zombie to turn to while it is in the alerted state 
// (i.e. seek left or right or don't seek)
public enum SeekingType {  Left = -1, None = 0, Right = 1 }

/// <summary>
/// State machine for all zombies.
/// </summary>
public class AiZombieStateMachine : AiStateMachine {

   // the value to use as being full (i.e. not hungry)
   private const float MAX_SATISFACTION = 1f;

   // Animator hashes (improves animator lookup performance)
   private static readonly int SPEED_HASH = Animator.StringToHash("speed");
   private static readonly int FEEDING_HASH = Animator.StringToHash("feeding");
   private static readonly int SEEKING_HASH = Animator.StringToHash("seeking");
   private static readonly int ATTACK_HASH = Animator.StringToHash("attack");

   // carves a section out of the sensor circle (like a piece of pie)
   [SerializeField][Range(10f,360f)] private float fieldOfView = 50.0f; // in degrees

   // how far in percentage the zombie can see in its fov (i.e. affects the distance the zombie can see)
   [SerializeField] [Range(0f, 1f)] private float sight = 0.5f; // percentage

   // how far in percentage the zombie can hear in all directions (i.e. affects the distance the zombie can hear)
   [SerializeField] [Range(0f, 1f)] private float hearing = 1.0f; // percentage (1 = can hear full range of sensor trigger)

   // TODO:
   [SerializeField] [Range(0f, 1f)] private float agression = 0.5f; // percentage

   // the amount of health the zombie currently has (0 = dead)
   [SerializeField] [Range(0, 100)] private int health = 100;

   // when the zombie hears a sound, it stops and goes into an alerted state and tries to align itself with the sound
   // if the sound comes within the fov, it will persue it
   // intelligence gives it a greater chance to turn the correct way to track the source of the sound
   [SerializeField] [Range(0f, 1f)] private float intelligence = 0.5f;

   // used when the zombie is feeding or when it should feed (like hunger)
   // zombie gets weaker after moving around and will have to replenish
   [SerializeField] [Range(0f, MAX_SATISFACTION)] private float satisfaction = MAX_SATISFACTION; // max means they are not hungry (full)

   // rate at which satisfaciton increases
   [SerializeField] private float replenishRate = 2.0f;

   // rate at which satisfaciton decreases (ticked off as the zombie exerts itself)
   [SerializeField] private float depletionRate = 0.1f;

   // ===========================
   // the following is used to maintain the current state and it is closely linked to the animator controller parameters
   // ===========================

   // which direction do we want to seek - invokes a turn in the animator
   [SerializeField] private SeekingType seeking = SeekingType.None;

   // Feeding is a parameter in our animator controller
   private bool feeding = false;

   // Crawling is a parameter in the animator controller
   // only can crawl when lower body has been damaged so it cannot walk anymore
   private bool crawling = false;

   // Attack is a parameter in the animator controller
   private int attackType = 0;

   // we were going to use the speed from the NavMeshAgent under steering (now we control it)
   private float speed = 0.0f;

   // getters that are not set by child states
   public float FieldOfView { get { return this.fieldOfView; } }
   public float Hearing { get { return this.hearing; } }
   public float Sight { get { return this.sight; } }
   public bool Crawling { get { return this.crawling; } }
   public float Intelligence { get { return this.intelligence; } }

   // getters/setters
   public float ReplenishRate { get { return this.replenishRate; } }
   public float Satisfaction { get { return this.satisfaction; } set { this.satisfaction = value; } }
   public float Agression { get { return this.agression; } set { this.agression = value; } }
   public int Health { get { return this.health; } set { this.health = value; } }
   public int AttackType { get { return this.attackType; } set { this.attackType = value; } }
   public bool Feeding { get { return this.feeding; } set { this.feeding = value; } }
   public SeekingType Seeking { get { return this.seeking; } set { this.seeking = value; } }
   public float Speed { get { return this.speed; } set { this.speed = value; } }

   /// <summary>
   /// Monobehavior life-cycle callback method that is invoked each frame. 
   /// </summary>
   protected override void Update() {
      // delegate to parent (parent will check to see if the state changed)
      base.Update(); 
      DepleteSatisfaction();
      UpdateAnimator();
   }

   // TODO: create an enum with attributes for param names (similar to enums in java)
   // TODO: use hashCode method for animator parameter sets
   /// <summary>
   /// Updates the appropriate parameters in the Animator (i.e. in the animator controller "Omni Zombie 1" for example)
   /// </summary>
   private void UpdateAnimator() {
      // update the animator
      this.Animator.SetFloat(SPEED_HASH, this.speed);
      this.Animator.SetBool(FEEDING_HASH, this.feeding);
      this.Animator.SetInteger(SEEKING_HASH, (int) this.seeking);
      this.Animator.SetInteger(ATTACK_HASH, this.attackType);
   }

   /// <summary>
   /// Indicates whether or not the zombie is hungry enough and can satisfy that hunger because the given target is within range.
   /// </summary>
   /// <param name="target">Should represent a visual food target.</param>
   /// <returns>True if the zombie is hungry enough and can reach the target.</returns>
   public bool CanHungerBeSatisfied(AiTarget target) {
      if (target.Type != AiTargetType.Visual_Food) {
         Debug.LogWarning("Received invalid target when checking to see if hunger could be satisfied: " + target.ToString());
         return false;
      }
      return (MAX_SATISFACTION - Satisfaction) > (target.Distance / Sensor.WorldRadius);
   }

   /// <summary>
   /// Indicates whether or not the zombie is hungry. 
   /// </summary>
   /// <returns></returns>
   public bool IsHungery() {
      // give it some floating point wiggle room (hence the -0.1f)
      return Satisfaction <= (MAX_SATISFACTION - 0.1f);
   }

   /// <summary>
   /// Depletes the current satisfaction value until it reaches the lower threshold.  Satisfaction is how hungry the zombie is.
   /// </summary>
   private void DepleteSatisfaction() {
      this.satisfaction = Mathf.Max(0, this.satisfaction - ((this.depletionRate * Time.deltaTime) / 100.0f) * Mathf.Pow(speed, 3));
   }

   /// <summary>
   /// Sets the seeking value based upon the provided int value.  The original type of seeking was an int.  I changed
   /// it to an enum to give it a little more clarity for me.  Doing so required this additional method that I was not
   /// anticipating.  The sign of an angle was used to determine the seeking direction.  With a Java enum one could
   /// build all the logic into the enum.  
   /// </summary>
   /// <param name="value"></param>
   public void SetNumericSeeking(int value) {
      if (value == 0) {
         this.seeking = SeekingType.None;
      } else {
         this.seeking = value > 0 ? SeekingType.Right : SeekingType.Left;
      }
   }
}
