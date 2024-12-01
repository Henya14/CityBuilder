using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [SerializeField] private string m_pathOfResourceRecipeFile;
    //[SerializeField]
    //public List<Resource> resources = new List<Resource>();
    private bool m_logAsError=true;
    [SerializeField]
    private List<string> m_resourcesWithSpecialBuilding;

    // Start is called before the first frame update
    void Start()
    {
        //ResourceImporter.Save();
        //resources=ResourceImporter.GetResources();
        //ResourceImporter.GetResources(this, m_pathOfResourceRecipeFile);
        ResourceImporter.LoadResources(this, m_pathOfResourceRecipeFile);
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
    public void AddNewResource(string name,string description, float amount_per_hour, Dictionary<string, float> recipe)
    {
        var o = new GameObject(name);
        o.transform.parent = this.transform;
        Resource.CreateComponent(o, name, description,amount_per_hour, recipe);
    }
    public List<string> GetAllResourceName()
    {
        return GetComponentsInChildren<Resource>()
                    .Select(res => res.ResourceName)
                    .ToList();
    }
    public List<string> GetRawResourceName()
    {
        return FindObjectOfType<RawMaterialManager>().GetRawMaterials()
                    .Select(raw => raw.Type)
                    .ToList();
    }
    public List<string> GetNotRawResourceName()
    {
        var rawlist = GetRawResourceName();
        return GetAllResourceName()
                    .Where(res => !rawlist.Contains(res))
                    .ToList();
    }
    public List<string> GetNotRawResourceNameWithoutSpecial()
    {
        var rawlist = GetRawResourceName();
        return GetAllResourceName()
                    .Where(res => !rawlist.Contains(res) && !m_resourcesWithSpecialBuilding.Contains(res))
                    .ToList();
    }
}
