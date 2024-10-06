using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractRawMaterial : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


[System.Serializable]
public struct RawMaterialWithRearity 
{
    public string Type;
    public float Rearity;
    public Color Color;

    public RawMaterialWithRearity(string type, float rearity, Color color) : this()
    {
        this.Type = type;
        this.Rearity = rearity;
        this.Color = color;
    }
}