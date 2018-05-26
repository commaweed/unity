using UnityEngine;

public class CameraRaycaster1 : MonoBehaviour {

   [SerializeField]
   private Layer[] layerPriorities = {
        Layer.Enemy,
        Layer.Walkable
    };

   // maximum distance to continue raycasting
   [SerializeField]
   private float distanceToBackground = 100f;

   private Camera mainCamera;

   private HitItemMetadata currentHitItemMetadata = null;

   // configure observer pattern for listeners when layerHit state changes
   public delegate void OnHitItemChange(HitItemChangeEvent itemHitChangeEvent);
   public event OnHitItemChange OnHitItemChangeObservers;

   void Start() {
      mainCamera = Camera.main;
      if (Camera.main == null) {
         Debug.LogWarning(
             "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\"," +
             " for camera-relative controls.", gameObject);
      }
   }

   private void Update() {
      HandleRayCasting();
   }

   /// <summary>
   /// Handles ray casting.
   /// </summary>
   private void HandleRayCasting() {
      HitItemChangeEvent changeEvent = new HitItemChangeEvent();
      if (this.currentHitItemMetadata != null) {
         changeEvent.OldItem = new HitItemMetadata {
            ItemHit = this.currentHitItemMetadata.ItemHit,
            LayerHit = this.currentHitItemMetadata.LayerHit
         };
      }

      // cycle through the layer priorities to see if one of them was hit by priority order
      foreach (Layer layer in layerPriorities) {
         RaycastHit? hitInfo = RaycastForLayer(layer);
         if (hitInfo != null && hitInfo.HasValue) {
            changeEvent.NewItem = new HitItemMetadata {
               ItemHit = hitInfo.Value,
               LayerHit = layer
            };
            break;
         }
      }

      // update state to the new value
      this.currentHitItemMetadata = changeEvent.NewItem;

      // notify listeners 
      if (OnHitItemChangeObservers != null) {
         OnHitItemChangeObservers(changeEvent);
      }
   }

   /// <summary>
   /// Shoots a ray from maincamera through mouse click and determines if it hit the given layer.  
   /// </summary>
   /// <param name="layer">The layer to test.</param>
   /// <returns>If the layer was hit, it will return information about the item that was hit; otherwise, it returns null.</returns>
   RaycastHit? RaycastForLayer(Layer layer) {
      // bit mask for the various layers in the game (used to ignore other layers)
      int layerMask = 1 << (int)layer;

      // Create a ray from camera through mouse position 
      Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

      // cast the ray and determine if it hit the given layer; if so return the information about the item 
      RaycastHit hit; // used as an out parameter 
      bool hasHit = Physics.Raycast(ray, out hit, distanceToBackground, layerMask);

      return hasHit ? hit : (RaycastHit?)null;
   }
}
