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

   /// <summary>
   /// Callback for what to render in the scene.
   /// </summary>
   private void OnSceneGUI() {
      AiWaypointNetwork network = (AiWaypointNetwork) target;
      DrawWaypointLabels();

      switch (network.DisplayMode) {
         case WaypointDisplayMode.Lines:
            DisplayLines();
            break;
         case WaypointDisplayMode.Paths:
            DisplayPaths();
            break;
         default:
            break;
      }
   }

   /// <summary>
   /// Displays connecting lines in the scene.
   /// </summary>
   private void DisplayLines() {
      AiWaypointNetwork network = (AiWaypointNetwork) target;
      List<Vector3> linePoints = new List<Vector3>();
      Vector3? nonNullStartPosition = null;

      // store non-null connections 0 -> N
      for (int i = 0; i < network.Waypoints.Count; i++) {
         if (network.Waypoints[i] != null) {
            linePoints.Add(network.Waypoints[i].position);
            // save reference to first non-null position so we can connect it to the end (close loop)
            if (nonNullStartPosition == null) {
               nonNullStartPosition = network.Waypoints[i].position;
            }
         }
      }

      // close the loop by adding start to the end of the list 
      // start is at beginning and end 
      if (nonNullStartPosition != null) {
         linePoints.Add((Vector3) nonNullStartPosition);
      }

      // render our closed network to the scene view
      Handles.color = Color.cyan;
      Handles.DrawPolyLine(linePoints.ToArray());
   }

   /// <summary>
   /// Draw the waypoint labels
   /// </summary>
   private void DrawWaypointLabels() {
      AiWaypointNetwork network = (AiWaypointNetwork) target;
      for (int index = 0; index < network.Waypoints.Count; index++) {
         if (network.Waypoints[index] != null) {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            Handles.Label(network.Waypoints[index].position, "P" + index, style);
            Handles.Label(network.Waypoints[index].position, "\n" + network.Waypoints[index].name, style);
         }
      }
   }

   /// <summary>
   /// Displays nav mesh paths in the scene.
   /// </summary>
   private void DisplayPaths() {
      AiWaypointNetwork network = (AiWaypointNetwork) target;
      WaypointEngine engine = new WaypointEngine(network);
      Waypoint fromWaypoint = engine.GetWaypoint(network.PathStartIndex);
      Waypoint toWaypoint = engine.GetWaypoint(network.PathEndIndex);
      if (fromWaypoint != null && fromWaypoint.Transform != null && toWaypoint != null && toWaypoint.Transform != null) {
         NavMeshPath path = new NavMeshPath();
         NavMesh.CalculatePath(fromWaypoint.Transform.position, toWaypoint.Transform.position, NavMesh.AllAreas, path);
         Handles.color = Color.yellow;
         Handles.DrawPolyLine(path.corners);
      }
   }

}
