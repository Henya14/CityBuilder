using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    //[SerializeField]
    //public List<Resource> resources = new List<Resource>();
    private bool m_logAsError=true;

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
        if(m_logAsError) Debug.LogError(name + " Resource not found by " + this.name);
        return null;
    }
    public void AddResourceAsRaw(RawMaterialWithRearity rawResource)
    {
        m_logAsError=false;
        if (this.FindResourceByName(rawResource.Type) != null)
        {
            m_logAsError = true;
            return;//Cause its already added
        }
        m_logAsError = true;
        var o = new GameObject(rawResource.Type);
        o.transform.parent = this.transform;
        Resource.CreateComponent(o, rawResource.Type, "", rawResource.GatheredAmountPerHour, new Dictionary<string, float>());

    }
}
