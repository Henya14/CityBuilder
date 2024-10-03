using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadDrawer))]
public class RoadDrawerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RoadDrawer pd = (RoadDrawer)target;

        if (DrawDefaultInspector())
        {
            if (pd.autoUpdateEnabled)
            {
                pd.DrawRoadCurve();
            }
        }
    }

}
