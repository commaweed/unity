using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

/// <summary>
/// Custom Editor for the AiWaypointNetwork script component.
/// </summary>
[CustomEditor(typeof(AiWaypointNetwork))]
public class AiWaypointNetworkEditor : Editor {

   /// <summary>
   /// Callback for what to render in the scene.
   /// </summary>
   private void OnSceneGUI() {
      AiWaypointNetwork network = (AiWaypointNetwork) target;
      if (network.DisplayMode == WaypointDisplayMode.Lines) {
         displayLines();
      } else {
         displayPaths();
      }

      Debug.Log(network.GetFirstNonNullWaypoint(-2));
   }

   /// <summary>
   /// Displays connecting lines in the scene.
   /// </summary>
   private void displayLines() {
      AiWaypointNetwork network = (AiWaypointNetwork) target;
      List<Vector3> linePoints = new List<Vector3>();
      Vector3? nonNullStartPosition = null;

      // create the labels on the scene and populate the linePoints
      for (int i = 0; i < network.Waypoints.Count; i++) {
         if (network.Waypoints[i] != null) {
            Handles.color = Color.white;
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            Handles.Label(network.Waypoints[i].position, "P" + i, style);
            linePoints.Add(network.Waypoints[i].position);
            if (nonNullStartPosition == null) {
               nonNullStartPosition = network.Waypoints[i].position;
            }
         }
      }

      // close the linePoints loop to the first non-null value
      if (nonNullStartPosition != null) {
         linePoints.Add((Vector3) nonNullStartPosition);
      }

      // connect all the non-null waypoints with lines
      Handles.color = Color.cyan;
      Handles.DrawPolyLine(linePoints.ToArray());
   }

   /// <summary>
   /// Displays nav mesh paths in the scene.
   /// </summary>
   private void displayPaths() {
      AiWaypointNetwork network = (AiWaypointNetwork) target;

      Transform fromTransform = network.GetStartWaypoint();
      Transform toTransform = network.GetEndWaypoint();
      if (fromTransform != null && toTransform != null) {
         NavMeshPath path = new NavMeshPath();
         NavMesh.CalculatePath(fromTransform.position, toTransform.position, NavMesh.AllAreas, path);
         Handles.color = Color.yellow;
         Handles.DrawPolyLine(path.corners);
      }
   }

   /// <summary>
   /// Callback for what to render in the inspector.
   /// </summary>
   public override void OnInspectorGUI() {
      AiWaypointNetwork network = (AiWaypointNetwork) target;

      network.DisplayMode = (WaypointDisplayMode) EditorGUILayout.EnumPopup("Display Mode", network.DisplayMode);

      if (network.DisplayMode == WaypointDisplayMode.Paths) {
         network.PathStartIndex = EditorGUILayout.IntSlider("Start Index", network.PathStartIndex, 0, network.Waypoints.Count - 1);
         network.PathEndIndex = EditorGUILayout.IntSlider("End Index", network.PathEndIndex, 0, network.Waypoints.Count - 1);
      }

      // display the default behavior for all non hidden serialized fields
      DrawDefaultInspector();
   }
}
