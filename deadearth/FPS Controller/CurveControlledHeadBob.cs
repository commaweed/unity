using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// the client animation curve callback types.
public enum ClientAnimationCurveCallBackType {
   Horizontal, // event should be processed by the X playhead
   Vertical    // event should be processed by the Y playhead
}

// Delegate, used by clients to register with this delegate their callback function
public delegate void HeadBobCallback();

// 
/// <summary>
/// Represents a single event on our animation curve timeline.  Clients can register events with our animation timeline.
/// </summary>
[System.Serializable]
class ClientAnimationCurveEvent {
   // when the client wants the event to be triggered
   public float Time = 0.0f;

   // the callback function the client wants to register
   public HeadBobCallback Function = null;

   // default event type
   public ClientAnimationCurveCallBackType Type = ClientAnimationCurveCallBackType.Vertical;
}

/// <summary>
/// Represents the controller for the head bob animation that we see when the player walks or runs.  We do this by using
/// an animation curve and we evaluate the position on the curve over time as a Vectro3.  The FPSController is what
/// requests it and it will use it to position the main camera (i.e. camera.transform.localPosition)
/// </summary>
[System.Serializable]
public class CurveControlledHeadBob  {

   [SerializeField]
   AnimationCurve bobcurve 
      // defines a basic sine-wave shape
      = new AnimationCurve(
         new Keyframe(0f, 0f), 
         new Keyframe(0.5f, 1f),
         new Keyframe(1f, 0f), 
         new Keyframe(1.5f, -1f),
         new Keyframe(2f, 0f) // the last keyframe (time = 2f)
      );

   // the higher the value, the more pronounced the head bob will be
   [SerializeField] float horizontalMultiplier = 0.01f;

   // this should be less strong than the up/down one
   [SerializeField] float verticalMultiplier = 0.02f;

   // like a multiplier because we are doing head bobs for both veritcal and horizontal play heads
   // it is how fast the y-playhead should move compared to the x-playhead (at the moment, 2 times as much)
   [SerializeField] float verticaltoHorizontalSpeedRatio = 2.0f;

   // allows us to speed up or slow down the bob
   // used to control the speed at which we step along the horizontal axis of the animation curve with each update
   // the bigger the size, the slower we will move along our time line
   [SerializeField] float baseInterval = 1.0f;

   // used the fetch the time of the last keyframe (we can see it is 2f right now)
   private float curveEndTime;

   // responsible for horizontal bobbing (left/right)
   private float xPlayHead;
   private float prevXPlayHead;  // used to know when to trigger events

   // responsible for vertical bobbing (up/down)
   private float yPlayHead;
   private float prevYPlayHead;  // used to know when to trigger events

   // all registered events and collected 
   private List<ClientAnimationCurveEvent> events = new List<ClientAnimationCurveEvent>();

   public void Initialize() {
      // Record time length of bob curve
      // get the last keyframe in the curve and give me its time (right now it is 2f)
      curveEndTime = bobcurve[bobcurve.length - 1].time;

      xPlayHead = 0.0f;
      yPlayHead = 0.0f;
      prevXPlayHead = 0.0f;
      prevYPlayHead = 0.0f;
   }

   /// <summary>
   /// Register events with the animation curve so that certain external actions can occur.  Each of these events will provide
   /// a callback function located in the client that provided the event and it will be triggered when the given time happens
   /// on the animation curve.  
   /// We currently use this to make a sound of foot steps using the animaation curve.
   /// </summary>
   /// <param name="time"></param>
   /// <param name="function"></param>
   /// <param name="type"></param>
   public void RegisterEventCallback(float time, HeadBobCallback function, ClientAnimationCurveCallBackType type) {
      // TODO: validate (ensure time is within bounds, function exists, etc.)
      ClientAnimationCurveEvent ccbeEvent = new ClientAnimationCurveEvent {
         Time = time,
         Function = function,
         Type = type
      };
      events.Add(ccbeEvent);

      // bubble up the smallest time events to the top of the list
      // TODO: see if there is a way store in a collection that stores elements in sorted order when they are added (e.g. java TreeSet) - for learning
      events.Sort(
         delegate (ClientAnimationCurveEvent t1, ClientAnimationCurveEvent t2) {
            return (t1.Time.CompareTo(t2.Time));
         }
      );
   }

   // call every frame by fps controller
   // add this vector to the local space of the camera
   public Vector3 GetLocalSpaceOffset(float speed) {
     
      xPlayHead += (speed * Time.deltaTime) / baseInterval;

      yPlayHead += ((speed * Time.deltaTime) / baseInterval) * verticaltoHorizontalSpeedRatio;

      // wrap them back around if we go over the keyframe time bounds
      if (xPlayHead > curveEndTime) xPlayHead -= curveEndTime;
      if (yPlayHead > curveEndTime) yPlayHead -= curveEndTime;

      ProcessEvents();

      // determine the valuke of (x,y) on the curve and then multiply each by its intensity (how big we want each to be)
      float xPos = bobcurve.Evaluate(xPlayHead) * horizontalMultiplier;
      float yPos = bobcurve.Evaluate(yPlayHead) * verticalMultiplier;

      // update the members
      this.prevXPlayHead = xPlayHead;
      this.prevYPlayHead = yPlayHead;

      return new Vector3(xPos, yPos, 0f);
   }

   /// <summary>
   /// Process any events that are piggy-backing on our head bob animation curve so that we can take action on them.
   /// An example of an event is to play a foot sound when the curve is at the bottom of each curve (i.e. at a tangent)
   /// </summary>
   private void ProcessEvents() {
      foreach (ClientAnimationCurveEvent clientEvent in this.events) {
         if (clientEvent.Type == ClientAnimationCurveCallBackType.Vertical) {
            if ((prevYPlayHead < clientEvent.Time && yPlayHead >= clientEvent.Time) ||
               (prevYPlayHead > yPlayHead && (clientEvent.Time > prevYPlayHead || clientEvent.Time <= yPlayHead))) {
               clientEvent.Function();
            }
         } else {
            if ((prevXPlayHead < clientEvent.Time && xPlayHead >= clientEvent.Time) ||
               (prevXPlayHead > xPlayHead && (clientEvent.Time > prevXPlayHead || clientEvent.Time <= xPlayHead))) {
               clientEvent.Function();
            }
         }
      }
   }


}
