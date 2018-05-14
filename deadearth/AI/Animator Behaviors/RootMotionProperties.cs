using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents all the properties pertaining to tracking root motion.  Root motion is something the 
/// animations can control.  Basically we accumulate the values as the animator changes animations.
/// An overall positive number means to apply the motion type and the animator will handle it,
/// otherwise, the navmesh system will handle it (or our code will).  
/// The AiStateMachine will hold a reference of this and interact with it as needed.
/// </summary>
public struct RootMotionProperties {

   private int rootPositionRefCount;
   private int rootRotationRefCount;

   /// <summary>
   /// Each time animator state changes, increment or decrement the counter by the given value.
   /// </summary>
   /// <param name="value">-1 or 1</param>
   public void IncrementRootPositionRefCount(int value) {
      this.rootPositionRefCount += value;
   }

   /// <summary>
   /// Each time animator state changes, increment or decrement the counter by the given value.
   /// </summary>
   /// <param name="value">-1 or 1</param>
   public void IncrementRootRotationRefCount(int value) {
      this.rootRotationRefCount += value;
   }

   /// <summary>
   /// Indicates whether or not our code should handle the position or whether the animation should.
   /// If positive, the animator will handle it.
   /// </summary>
   /// <returns>True, if the Animator's selected animation should handle the root position.</returns>
   public bool ShouldUseRootPosition {
      get { return this.rootPositionRefCount > 0; }
   }
   /// <summary>
   /// Indicates whether or not our code should handle the rotation or whether the animation should.
   /// If positive, the animator will handle it.  In general, we want to handle the walk, jog, run 
   /// ourselves.  But idling and turning can be done by the animations themselves.
   /// </summary>
   /// <returns>True, if the Animator's selected animation should handle the root rotation.</returns>
   public bool ShouldUseRootRotation {
      get { return this.rootRotationRefCount > 0; }
   }
}
