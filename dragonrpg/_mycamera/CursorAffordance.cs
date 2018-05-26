using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rpg.CameraUi {
   [RequireComponent(typeof(CameraRaycaster))]
   public class CursorAffordance : MonoBehaviour {

      [SerializeField]
      private Vector2 cursorHotspot;

      [SerializeField]
      private Texture2D walkCursor;

      [SerializeField]
      private Texture2D attackCursor;

      [SerializeField]
      private Texture2D unknownCursor;

      private CameraRaycaster rayCaster;

      // Use this for initialization
      void Start() {
         RegisterLayerChange();

         // set cursor hotspot to the top left corner of any of the images of any of the images
         this.cursorHotspot = new Vector2(0, 0); // top left corner
      }

      private void RegisterLayerChange() {
         //rayCaster = FindObjectOfType<CameraRaycaster>();
         rayCaster = GetComponent<CameraRaycaster>();

         // register with the CameraRaycaster that we are listening to layer hit changes
         this.rayCaster.notifyLayerChangeObservers += OnLayerChange;
      }

      /// <summary>
      /// Event handler that fires whenever the CameraRaycaster layer hit changes.
      /// </summary>
      private void OnLayerChange(int newLayer) {
         UpdateCursor(newLayer);
      }

      /// <summary>
      /// Updates the cursor based upon the current value of the raycaster layer that was hit.
      /// </summary>
      /// <param name="layerHit">The value of the layer that was hit.</param>
      void UpdateCursor(int layer) {
         switch (layer) {
            case 8:  // walkable
               Cursor.SetCursor(walkCursor, cursorHotspot, CursorMode.Auto);
               break;
            case 9:  // enemy
               Cursor.SetCursor(attackCursor, cursorHotspot, CursorMode.Auto);
               break;
            default:
               Cursor.SetCursor(unknownCursor, cursorHotspot, CursorMode.Auto);
               break;
         }
      }

      // Update is called once per frame
      void Update() {
      }
   }
}