using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CameraRaycaster1))]
public class CursorAffordance1 : MonoBehaviour {

   [SerializeField]
   private Texture2D walkCursor;

   [SerializeField]
   private Texture2D attackCursor;

   [SerializeField]
   private Vector2 cursorHotspot;

   [SerializeField]
   private Texture2D unknownCursor;


   private CameraRaycaster1 rayCaster;

   // Use this for initialization
   void Start() {
      //rayCaster = FindObjectOfType<CameraRaycaster>();
      rayCaster = GetComponent<CameraRaycaster1>();

      // register with the CameraRaycaster that we are listening to layer hit changes
      this.rayCaster.OnHitItemChangeObservers += OnHitItemChanged;

      // set cursor hotspot to the top left corner of any of the images of any of the images
      this.cursorHotspot = new Vector2(0, 0); // top left corner
   }

   /// <summary>
   /// Event handler that fires whenever the CameraRaycaster layer hit changes.
   /// </summary>
   private void OnHitItemChanged(HitItemChangeEvent changeEvent) {
      if (changeEvent.DidLayerChange()) {
         if (changeEvent.NewItem == null) {
            UpdateCursor(Layer.RaycastEndStop);
         } else {
            UpdateCursor(changeEvent.NewItem.LayerHit);
         }
      }
   }

   /// <summary>
   /// Updates the cursor based upon the current value of the raycaster layer that was hit.
   /// </summary>
   /// <param name="layerHit">The value of the layer that was hit.</param>
   void UpdateCursor(Layer layerHit) {
      print("layer changed " + layerHit);
      switch (layerHit) {
         case Layer.Walkable:
            Cursor.SetCursor(walkCursor, cursorHotspot, CursorMode.Auto);
            break;
         case Layer.Enemy:
            Cursor.SetCursor(attackCursor, cursorHotspot, CursorMode.Auto);
            break;
         case Layer.RaycastEndStop:
            Cursor.SetCursor(unknownCursor, cursorHotspot, CursorMode.Auto);
            break;
      }
   }

   // Update is called once per frame
   void Update() {
   }
}
