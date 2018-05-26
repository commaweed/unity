using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// this will only execute while the GameObject with the MapGenerator script is selected (and expanded)
// it will cause a generated map to be present without the game actually running
[CustomEditor(typeof(MapGenerator))]
public class MapEditor : Editor {

   public override void OnInspectorGUI() {
      MapGenerator map = target as MapGenerator;

      // if value has been updated in inspector
      if (DrawDefaultInspector()) {
         map.GenerateMap();
      }

      // if we press the button
      if (GUILayout.Button("Generate Map")) {
         map.GenerateMap();
      }
   }
}
