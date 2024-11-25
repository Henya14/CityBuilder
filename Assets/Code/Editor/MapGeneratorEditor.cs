using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(MapGenarator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenarator mapGenarator = (MapGenarator)target;

        if (DrawDefaultInspector())
        {
            if (mapGenarator.autoUpdateEnabled)
            {
                mapGenarator.GenerateMap();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            mapGenarator.GenerateMap();
        }
    }

}
