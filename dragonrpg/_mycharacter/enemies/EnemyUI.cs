using UnityEngine;

namespace Rpg.Character {
   // Add a UI Socket transform to your enemy
   // Attack this script to the socket
   // Link to a canvas prefab that contains NPC UI
   public class EnemyUI : MonoBehaviour {

      // Works around Unity 5.5's lack of nested prefabs
      [Tooltip("The UI canvas prefab")]
      [SerializeField]
      GameObject enemyCanvasPrefab = null;

      Camera cameraToLookAt;

      // Use this for initialization 
      void Start() {

         if (enemyCanvasPrefab == null) {
            throw new System.ArgumentException("Missing the enemyCanvasPrefab; did you forget to add it to the UISocket?");
         }

         cameraToLookAt = Camera.main;
         Instantiate(enemyCanvasPrefab, transform.position, Quaternion.identity, transform);
      }

      // Update is called once per frame 
      void LateUpdate() {
         transform.LookAt(cameraToLookAt.transform);
         transform.rotation = Quaternion.LookRotation(cameraToLookAt.transform.forward);
      }
   }
}