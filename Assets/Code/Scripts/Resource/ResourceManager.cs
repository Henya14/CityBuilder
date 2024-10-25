using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    //[SerializeField]
    //public List<Resource> resources = new List<Resource>();

    // Start is called before the first frame update
    void Start()
    {
        //ResourceImporter.Save();
        //resources=ResourceImporter.GetResources();
        ResourceImporter.GetResources(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public Resource FindResourceByName(string name)
    {
        foreach(var res in GetComponentsInChildren<Resource>())
        {
            if (res.ResourceName == name) return res;
        }
        return null;
    }
}
