using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ResourceProducer))]
public class ResourceProducerEditor: Editor
{
    public override void OnInspectorGUI()
    {
        ResourceProducer resourceProducer = (ResourceProducer)target;

        if (DrawDefaultInspector())
        {
        }

        if (GUILayout.Button("Assign"))
        {
            resourceProducer.AssignToCurrentType();
        }
        if (GUILayout.Button("Switch"))
        {
            resourceProducer.Switch();
        }
    }

}