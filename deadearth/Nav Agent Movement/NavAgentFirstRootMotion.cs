using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

/// <summary>
/// Root motion is enabled but applied by our own script.
/// 
/// NavMeshAgent only updates Position (not rotation).
/// 
/// Current speed and turn rate of NavMeshAgent is computed and passed to the animator as parameters.  These 
/// are used to find the correct animation blend that most closely matches.
/// 
/// Root rotation directly applied to transform via script.
/// Root motion used to override NavMeshAgent's current velocity.
/// 
/// No foot sliding at all but at the cost of navigation accuracy.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class NavAgentFirstRootMotion : MonoBehaviour {

   private const string HORIZONTAL_PARAM = "Horizontal";
   private const string VERTICAL_PARAM = "Vertical";
   private const string TURN_ON_SPOT_PARAM = "TurnOnSpot";

   private static readonly int HORIZONTAL_HASH = Animator.StringToHash(HORIZONTAL_PARAM);
   private static readonly int VERTICAL_HASH = Animator.StringToHash(VERTICAL_PARAM);
   private static readonly int TURNONSPOT_HASH = Animator.StringToHash(TURN_ON_SPOT_PARAM);

   [SerializeField]
   private AiWaypointNetwork network;

   private int turnOnSpot;

   // the navAgent original speed (max speed)
   private float originalMaxSpeed;

   [SerializeField]
   private bool hasPath;
   [SerializeField]
   private bool pathPending;
   [SerializeField]
   private bool isPathStale;
   [SerializeField]
   private float remainingDistance;
   [SerializeField]
   private string currentWaypointDisplay;

   private NavMeshAgent navAgent;
   private Animator animatorController;

   private Waypoint currentWaypoint;
   private WaypointEngine engine;

   // Use this for initialization
   void Start() {
      InitializeNavAgent();
      InitializeAnimatorController();
      InitializeWaypointDestination();
   }

   // Update is called once per frame
   void Update() {
      HandleNavAgentSpeed();
      HandleTurningAnimation();
      HandleVelocityAnimation();
      HandleFindNextWayPoint();
   }

   /// <summary>
   /// Initializes the NavAgent.
   /// </summary>
   private void InitializeNavAgent() {
      navAgent = GetComponent<NavMeshAgent>();
      navAgent.stoppingDistance = 1.0f;
      this.originalMaxSpeed = this.navAgent.speed;
   }

   /// <summary>
   /// Initializes the underlying Animator Controller.
   /// </summary>
   private void InitializeAnimatorController() {
      animatorController = GetComponent<Animator>();
      animatorController.runtimeAnimatorController = Resources.Load("FirstRootMotionAuthority") as RuntimeAnimatorController;
      animatorController.applyRootMotion = true;
   }

   /// <summary>
   /// Initializes the Waypoint stuff, to include setting the initialze direction.
   /// </summary>
   private void InitializeWaypointDestination() {
      Assert.IsNotNull(this.network, "NavAgentExample: 'Way Point Network' is missing; did you forget to set it in inspector!");

      engine = new WaypointEngine(this.network);

      if (this.currentWaypoint == null) {
         this.currentWaypoint = engine.GetWaypoint(network.PathStartIndex);
      }

      SetWaypointDestination(false);
   }

   /// <summary>
   /// Sets the waypoint destination for the nav agent
   /// </summary>
   /// <param name="shouldIncrement"></param>
   private void SetWaypointDestination(bool shouldIncrement) {
      if (this.network == null) { return; }

      currentWaypoint = (shouldIncrement ? currentWaypoint.NextWaypoint : currentWaypoint);
      
      if (currentWaypoint != null && currentWaypoint.Transform != null) {
         this.currentWaypointDisplay = currentWaypoint.ToString();
         this.navAgent.destination = currentWaypoint.Transform.position;
      }
   }

   /// <summary>
   /// Handles the turning animation.
   /// </summary>
   private void HandleTurningAnimation() {
      float horizontalValue = CalculateHorizontalValue();

      // set the horizontal value in the animator controller
      this.animatorController.SetFloat(HORIZONTAL_HASH, horizontalValue, 0.1f, Time.deltaTime);

      this.turnOnSpot = CalculateTurnOnSpotValue(horizontalValue);
      this.animatorController.SetInteger(TURNONSPOT_HASH, turnOnSpot);
   }

   /// <summary>
   /// Calculates the turn on spot value according to the provided horizontal value.
   /// </summary>
   /// <param name="horizontalValue">The horizontal position</param>
   /// <returns>If the agent is about to stop, it will be a signed value to indicate the turning
   /// direction.  Otherwise it will be 0.
   /// </returns>
   private int CalculateTurnOnSpotValue(float horizontalValue) {
      int result = 0;

      if (IsAboutToStopAndMakeASharpTurn()) {
         result = (int) Mathf.Sign(horizontalValue);
      } else {
         result = 0;
      }

      return result;
   }

   /// <summary>
   /// Sets the navAgent's speed according to whether or not it is about to stop.
   /// </summary>
   private void HandleNavAgentSpeed() {
      if (IsAboutToStopAndMakeASharpTurn()) {
         this.navAgent.speed = 0.1f;
      } else {
         this.navAgent.speed = this.originalMaxSpeed;
      }
   }

   /// <summary>
   /// Indicates whether or not the agent is about to stop and make a sharp turn.  This is, if navAgent is 
   /// about to stop and the angle between direction we are facing and direction of our desired velocity 
   /// is a steep turn.
   /// </summary>
   /// <returns>True if about to stop and make a sharp turn.</returns>
   private bool IsAboutToStopAndMakeASharpTurn() {
      bool isSlowing = this.navAgent.desiredVelocity.magnitude < 1.0f;
      bool isMakingSharpTurn = Vector3.Angle(this.transform.forward, this.navAgent.desiredVelocity) > 10.0f;
      return isSlowing && isMakingSharpTurn ;
   }

   /// <summary>
   /// Returns the horizontal animatorController value based upon the navAgents desiredVelocity.
   /// </summary>
   /// <returns>The horizontal value.</returns>
   private float CalculateHorizontalValue() {
      Vector3 crossProduct = Vector3.Cross(
         transform.forward,
         this.navAgent.desiredVelocity.normalized // unit length version of the resulting vector3
      );
      float horizontal = (crossProduct.y < 0) ? -crossProduct.magnitude : crossProduct.magnitude;

      // clamp it to our speed range (2.32 is from blend controller)
      return Mathf.Clamp(horizontal * 4.32f, -2.32f, 2.32f);
   }

   private void HandleVelocityAnimation() {
      this.animatorController.SetFloat(VERTICAL_HASH, this.navAgent.desiredVelocity.magnitude, 0.1f, Time.deltaTime);
   }

   /// <summary>
   /// Handles the finding of the next waypoint.
   /// </summary>
   private void HandleFindNextWayPoint() {
      this.hasPath = this.navAgent.hasPath;
      this.pathPending = this.navAgent.pathPending;
      this.isPathStale = this.navAgent.isPathStale;
      this.remainingDistance = this.navAgent.remainingDistance;

      //!this.hasPath
      if ((this.remainingDistance <= this.navAgent.stoppingDistance && !this.pathPending) || 
         this.navAgent.pathStatus == NavMeshPathStatus.PathInvalid
         /* || this.navAgent.pathStatus == NavMeshPathStatus.PathPartial */
      ) {
         SetWaypointDestination(true); // next waypoint
      } else if (this.isPathStale) {
         SetWaypointDestination(false); // current waypoint
      }
   }
}
