using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : AbstractBuildingType
{
    Dictionary<Vector3Int, AbstarctProperty> propertyMap = new Dictionary<Vector3Int, AbstarctProperty>();
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddProperty(Vector3Int position, AbstarctProperty property) {
        propertyMap.Add(position, property);
    }

    public void RemoveProperty(Vector3Int position) {
        propertyMap.Remove(position);
    }

    public override GameObject GetBuildingPrefabForPosition(Vector3Int position)
    {
        var property = propertyMap.GetValueOrDefault(position, null);
        if (property == null) {
            return base.GetBuildingPrefabForPosition(position);
        } else {
            return property.PropertyGameObject;
        }
    }
}
