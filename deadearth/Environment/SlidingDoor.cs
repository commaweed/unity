using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum DoorState {  Open, Animating, Closed }

/// <summary>
/// Gave sliding behavior to a door to simulate how the navmesh behaves with navmesh object and the "carve" checked.
/// This this is a dynamic object that can move and since the navigation mesh usually has to be baked up-front, we
/// needed to add a nav mesh obstacle and to set it's "carve" checked.  That and Gary demonstrated that we don't want
/// to continually compute the carve so he set up a timer to generate once the object was done moving.
/// </summary>
[RequireComponent(typeof(NavMeshObstacle))]
public class SlidingDoor : MonoBehaviour {

   // how far to slide
   [SerializeField]
   private float slidingDistance = 7.0f;

   // how long the move animation should take
   [SerializeField]
   private float duration = 2.0f; // change state in seconds

   // the movement follows a curve that we defined in the inspector
   [SerializeField]
   public AnimationCurve JumpCurve = new AnimationCurve();

   private Vector3 openPosition = Vector3.zero;
   private Vector3 closePosition = Vector3.zero;
   private DoorState state = DoorState.Closed;

   // Use this for initialization
   void Start () {
      this.closePosition = this.transform.position;
      this.openPosition = this.closePosition + (this.slidingDistance * this.transform.right);
	}
	
	// Update is called once per frame
	void Update () {
      // important, the DoorState.Animating is used to ensure that the navmesh "carving" does not occur until the door state is Open or Close
      if (Input.GetMouseButton(0) && this.state != DoorState.Animating) {
         StartCoroutine(MoveTheDoor(this.state == DoorState.Open ? DoorState.Closed : DoorState.Open));
      }
	}

   /// <summary>
   /// Asynchronously moves the door to it's new position according to the given door state (open or closed).
   /// The door has three states so that the navmesh will only carve (expensive) when the door has finished moving.
   /// </summary>
   /// <param name="state"></param>
   /// <returns></returns>
   IEnumerator MoveTheDoor(DoorState state) {
      this.state = DoorState.Animating;
      float time = 0f;
      Vector3 startPosition = (state == DoorState.Open) ? this.closePosition : this.openPosition;
      Vector3 endPosition = (state == DoorState.Open) ? this.openPosition : this.closePosition;

      while (time <= duration) {
         float t = time / duration;
         // lerp means to gradually move from startPosition towards endPosition over time (defined by the curve)
         // AnimationCurve.Evaluate(t) means to return the value on the curve at time "t"
         this.transform.position = Vector3.Lerp(startPosition, endPosition, JumpCurve.Evaluate(t));

         time += Time.deltaTime;

         // yield so other things can happen
         yield return null;
      }

      // complete the move
      this.transform.position = endPosition;
      this.state = state;
   }
}
