using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Route))]
public class RouteEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Route route = (Route)target;

        if (DrawDefaultInspector())
        {
        }

        if (GUILayout.Button("SetStations"))
        {
            route.SetStation();
        }
    }
}