using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System.Collections.Generic;


namespace Rpg.CameraUi {
   public class CameraRaycaster : MonoBehaviour {

      // INSPECTOR PROPERTIES RENDERED BY CUSTOM EDITOR SCRIPT
      [SerializeField]
      private int[] layerPriorities;

      // maximum distance to continue raycasting
      private float maxRaycastDepth = 100f;

      // So get ? from start with Default layer terrain
      int topPriorityLayerLastFrame = -1;

      private Camera mainCamera;

      // Setup delegates for broadcasting layer changes to other classes
      public delegate void OnCursorLayerChange(int newLayer); // declare new delegate type
      public event OnCursorLayerChange notifyLayerChangeObservers; // instantiate an observer set

      public delegate void OnClickPriorityLayer(RaycastHit raycastHit, int layerHit); // declare new delegate type
      public event OnClickPriorityLayer notifyMouseClickObservers; // instantiate an observer set

      void Start() {
         mainCamera = Camera.main;
         if (Camera.main == null) {
            Debug.LogWarning(
                "Warning: no main camera found. CameraRaycaster needs a Camera tagged \"MainCamera\"," +
                " for camera-relative controls.", gameObject);
         }
      }

      private void Update() {
         // Check if pointer is over an interactable UI element
         if (EventSystem.current.IsPointerOverGameObject()) {
            NotifyObserersIfLayerChanged(5);
            return; // Stop looking for other objects
         }

         // Raycast to max depth, every frame as things can move under mouse
         Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
         RaycastHit[] raycastHits = Physics.RaycastAll(ray, maxRaycastDepth);

         RaycastHit? priorityHit = FindTopPriorityHit(raycastHits);
         if (!priorityHit.HasValue) { // if hit no priority object
            NotifyObserersIfLayerChanged(0); // broadcast default layer
            return;
         }

         // Notify delegates of layer change
         var layerHit = priorityHit.Value.collider.gameObject.layer;
         NotifyObserersIfLayerChanged(layerHit);

         // Notify delegates of highest priority game object under mouse when clicked
         if (Input.GetMouseButton(0)) {
            notifyMouseClickObservers(priorityHit.Value, layerHit);
         }
      }

      void NotifyObserersIfLayerChanged(int newLayer) {
         if (newLayer != topPriorityLayerLastFrame) {
            topPriorityLayerLastFrame = newLayer;
            notifyLayerChangeObservers(newLayer);
         }
      }

      RaycastHit? FindTopPriorityHit(RaycastHit[] raycastHits) {
         // Form list of layer numbers hit
         List<int> layersOfHitColliders = new List<int>();
         foreach (RaycastHit hit in raycastHits) {
            layersOfHitColliders.Add(hit.collider.gameObject.layer);
         }

         // Step through layers in order of priority looking for a gameobject with that layer
         foreach (int layer in layerPriorities) {
            foreach (RaycastHit hit in raycastHits) {
               if (hit.collider.gameObject.layer == layer) {
                  return hit; // stop looking
               }
            }
         }
         return null; // because cannot use GameObject? nullable
      }
   }
}