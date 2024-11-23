using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Resources;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;
using static UnityEngine.UI.GridLayoutGroup;

public class ResourceProducer : ResourceStorage, TransportationDestination
{
    [SerializeField] public string Type = null;

    [SerializeField] //To see in inspector
    private bool m_turnedOn;
    public bool TurnedOn { get { return m_turnedOn; } private set { m_turnedOn = value; } }

    [SerializeField] //To see in inspector
    private bool m_isRunning;
    public bool IsRunning { get { return m_isRunning; } private set { m_isRunning = value; } }

    [SerializeField] //To see in inspector
    private bool m_somethingMissing;
    public bool SomethingMissing { get { return m_somethingMissing; } private set { m_somethingMissing = value; } }

    [SerializeField] //To see in inspector
    private string m_reason;
    public string Reason { get { return m_reason; } private set { m_reason = value; } }

    [SerializeField] //To see in inspector
    private Dictionary<string, float> m_forProcessing; //Resources for recipe

    [SerializeField] //To see in inspector
    private Dictionary<string, float> m_needMore; //(missing)Resources for recipe 

    [SerializeField] //To see in inspector
    private int m_cycleUntilReCheck;

    [SerializeField] //To see in inspector
    private List<string> m_produceOptions; //List of resources that can be produced by this instance of producer
    public List<string> ProduceOptions { get { return m_produceOptions; } }

    // Start is called before the first frame update
    void Start()
    {
        m_number = m_counter;
        m_counter++;
        TurnOff();
        m_cycleUntilReCheck = 0;
        m_forProcessing = new Dictionary<string, float>();
        m_needMore = new Dictionary<string, float>();
        if (!string.IsNullOrEmpty(Type))
        {
            this.AssignToResource(Type);
        }
        else if (name.Contains("mine"))
        {
            Building building = this.gameObject.GetComponent<Building>();
            if (building != null)
            {//tile szel = 4
                var rawmanager = FindObjectOfType<RawMaterialManager>();
                m_produceOptions = new List<string>();
                Vector3 topLeft, topRight, bottomLeft, bottomRight;
                GameObject gameObject = this.gameObject;
                Quaternion orirot;
                Vector3 oripos, oriscale;
                Vector2Int bdata= building.GetBuildingData().size;
                oriscale = new Vector3(4 * bdata.x, 0, 4 * bdata.y); //tile scale * builddata size
                gameObject.transform.GetPositionAndRotation(out oripos, out orirot);

                topLeft = oripos + orirot.normalized * new Vector3(-oriscale.x / 2, 0, -oriscale.z / 2);
                topRight = oripos + orirot.normalized * new Vector3(oriscale.x / 2, 0, -oriscale.z / 2);
                bottomRight = oripos + orirot.normalized * new Vector3(oriscale.x / 2, 0, oriscale.z / 2);
                bottomLeft = oripos + orirot.normalized * new Vector3(-oriscale.x / 2, 0, oriscale.z / 2);
                m_produceOptions = rawmanager.OverlapedRawMaterials(new List<Vector3> { topLeft, topRight, bottomRight, bottomLeft }.Select(c => new Vector2(c.x, c.z)).ToList<Vector2>());
                /*
                var gridmanager = FindObjectOfType<GridManager>();
                foreach (var pos in building.gridPositions)
                {
                    var tile = gridmanager.GetTileAtPosition(new Vector3Int(pos.x,0,pos.z));
                    if (tile == null) Debug.Log($"Not Found: {pos.x}, {pos.y}, {pos.z}");
                    else
                    {
                        var ovlrm = rawmanager.OverlapedRawMaterials(gridmanager.GetTileCorners(tile).Select(c => new Vector2(c.x, c.z)).ToList<Vector2>());
                        if (ovlrm != null)
                        {
                            foreach (var c in ovlrm)
                            {
                                if (!m_produceOptions.Contains(c))
                                { m_produceOptions.Add(c); }
                            }
                        }
                    }
                }
                */
                Type = m_produceOptions[0];
                this.AssignToResource(Type);
            }
        }
        else
        {
            m_produceOptions = FindObjectOfType<ResourceManager>().GetNotRawResourceNameWithoutSpecial();
            Type = m_produceOptions[0];
            this.AssignToResource(Type);
        }
        
        //Subscribe to hour or minute change
        TimeManager.OnMinuteChanged += this.Produce;
        Switch();

    }
    private void Update()
    {
        /*if (Type != null && Resource == null)
            this.AssignToResource(Type);
        */
    }
    [SerializeField]
    public void Switch()
    {
        if (TurnedOn)
        {
           TurnOff();
        }
        else
        {
            TurnOn();
        }
    }
    public void TurnOn()
    {
        if(base.Resource != null)
        {
            TurnedOn = true;
            RunProducer();
            return;
        }
        else if (AssignToCurrentType())
        {
            TurnedOn = true;
            RunProducer();
            return;
        }

        Reason = "Can't find resource";
    }
    public void TurnOff()
    {
        TurnedOn = false;
        IsRunning = false;
        Reason = "Turned OFF";
    }
    public bool AssignToCurrentType()
    {
        return this.AssignToResource(Type);
    }
    private void RunProducer()
    {
        if(m_cycleUntilReCheck <= 0)
        {
            CheckRecipe();
            if (SomethingMissing)
            {
                IsRunning = false;
                Reason = "Something missing for the recipe";
            }
            else
            {
                Reason = string.Empty;
                m_cycleUntilReCheck = 60;
                IsRunning = true;
            }
        }
        else
        {

            Reason = string.Empty;
            IsRunning = true;
        }

    }
    private void CheckRecipe()
    {
        bool stM = false;
        var recipe = Resource.GetRecipe();
        foreach(var item in recipe)
        {
            if (m_forProcessing.ContainsKey(item.Key)) //check if the producer has the necessary resource
            {
                if (m_forProcessing[item.Key] >= item.Value) //check if the producer has the necessary amount
                {
                    continue;
                }
            }
            NeedMore(item);
            stM = true;
        }
        SomethingMissing = stM;
    }
    private void NeedMore(KeyValuePair<string, float> item)
    {
        float amount = item.Value;
        string res = item.Key;
        if(m_forProcessing.ContainsKey(res)) 
        { 
            amount -= m_forProcessing[res];
        }
        if (m_needMore.ContainsKey(res))
        {
            m_needMore[res] = amount;
        }
        else
        {
            m_needMore.Add(res, amount);
        }
    }


    public void AddResourceForProcess(KeyValuePair<string, float> item)
    {
        float amount = item.Value;
        string res = item.Key;

        // Add to resources for processing dict.
        if (m_forProcessing.ContainsKey(res))
        {
            m_forProcessing[res] += amount;
        }
        else
        {
            m_forProcessing.Add(res, amount);
        }

        // Remove from resources needed to processing dict.
        if (m_needMore.ContainsKey(res))
        {
            m_needMore[res] -= amount;
            if (m_needMore[res] <= 0)
            {
                m_needMore.Remove(res);
            }
        }
        //If its turned on then try produce
        if (TurnedOn) RunProducer();

    }
    public new bool AssignToResource(string resource)
    {
        if(string.IsNullOrEmpty(resource)) { return false; }
        if(resource == Resource?.ResourceName) { return true; } //Don't need to reassign for the same type
        if(!m_produceOptions.Contains(resource)) { return false; } //Can only be set to resource that in the options
        Resource originalResource = base.Resource;
        bool re = base.AssignToResource(resource);
        if(re)
        {
            if (originalResource != null)
            {
                bool ckRemoveFalse = originalResource.RemoveProducer(this);
                if (!ckRemoveFalse)
                {
                    Debug.Log($"Failed to remove producer({this.gameObject.name}) from resource ({originalResource.ResourceName}) ");
                    base.AssignToResource(originalResource.ResourceName);
                    
                    return ckRemoveFalse;
                }
            }

            bool ckAddFalse = Resource.AddProducer(this);
            if (!ckAddFalse)
            {
                Debug.Log($"Failed to remove producer({this.gameObject.name}) from resource ({originalResource.ResourceName}) ");
                base.AssignToResource(originalResource.ResourceName);
                return ckAddFalse;
            }

            Debug.Log($"Producer({this.gameObject.name}) succesfully assigned to new resource ({Resource.ResourceName})");
            m_cycleUntilReCheck = 0;
            return true;
        }
        else
        {
            if(originalResource == null)
            {
                Debug.Log($"Failed to assign producer({this.gameObject.name}) STORAGE for first time.");
            }
            else
            {
                Debug.Log($"Failed to remove producer({this.gameObject.name}) STORAGE from resource ({originalResource.ResourceName}) ");
            }
            //Debug.Log($"Failed to remove producer({this.name}) STORAGE from resource ({originalResource.ResourceName}) ");
        }
        return false;
    }
    public void Produce()
    {
        if(!TurnedOn)
        {
            return;
        }
        if(Resource != null && IsRunning)
        {
            if (m_cycleUntilReCheck > 0)
            {
                float rate = Resource.GetRatePerMinute();
                bool isFull = !base.AddResource(rate); //If Add not successfull that means it full
                if (isFull)
                {
                    IsRunning = false;
                    Reason = "Storage full";
                }
                else
                {
                    m_cycleUntilReCheck--;
                    foreach(var item in Resource.GetRecipe()) //Use ingerdient
                    {
                        m_forProcessing[item.Key] -= (item.Value / 60);
                    }
                }
            }
            else
            {
                RunProducer();
            }
        }

    }
    protected override void NewName(string newName)
    {
        string name = newName + " Producer " + m_number.ToString();
        this.gameObject.name = name;
        this.GetComponent<SelectableObject>().SetDescription(name);

    }
    public void AddOptions(string newOption)
    {
        if (m_produceOptions==null)
        {
            m_produceOptions = new();
        }
        m_produceOptions.Add(newOption);
    }

    public new bool Deliver(string type, float amount)
    {
        this.AddResourceForProcess(new KeyValuePair<string, float>(type, amount));
        return true;
    }
    public new bool Acceptable(string type)
    {
        return Resource.GetRecipe().ContainsKey(type);
    }
}
