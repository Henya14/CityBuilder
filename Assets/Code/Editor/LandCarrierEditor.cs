using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LandCarrier))]
public class LandCarrierEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LandCarrier carrier = (LandCarrier)target;

        if (DrawDefaultInspector())
        {
        }

        if (GUILayout.Button("Transport"))
        {
            carrier.Transport();
        }
    }
}