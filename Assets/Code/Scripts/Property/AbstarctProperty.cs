using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AbstarctProperty : MonoBehaviour
{
    public int Capacity { get; private set; } = 4;
    public int HeadCount { get; private set; } = 0;
    [SerializeField] public PropertyType PropertyType { get; protected set; }
    public int MaxCapacity { get; protected set; } = 100;

    public HouseLevel HouseLevel { get; set; }
    public GameObject PropertyGameObject { get; set; }
    public SelectionManager SelectionManager { get; set; }

    protected Light buildingLight;

    protected float intensity = 15f;


    // Start is called before the first frame update
    protected virtual void Start()
    {

        if (buildingLight == null)
        {
            // var lightGameObject = gameObject.transform.Find("BuildingLight")?.gameObject;
            // if (lightGameObject == null)
            // {
            //     lightGameObject = new GameObject("BuildingLight");
            //     lightGameObject.transform.SetParent(transform);
            //     lightGameObject.transform.localPosition = new  Vector3(0.0f, 0.2f, 0.0f);
            // }
            // buildingLight = lightGameObject.GetComponent<Light>();
            // if (buildingLight == null)
            // {
            //     // buildingLight = lightGameObject.AddComponent<Light>();
                
            // }
        }
        // buildingLight.type = LightType.Point;
        // buildingLight.color = new Color(0.69f, 0.29f, 0.1f);
        // buildingLight.range = 2f;
        // buildingLight.intensity = intensity;
        // TimeManager.OnHourChanged += OnHourChanged;
    }

    void OnHourChanged()
    {
        // if (TimeManager.Hour >= 18 || TimeManager.Hour < 6)
        // {
        //     buildingLight.intensity = intensity;
        // }
        // else
        // {
        //     buildingLight.intensity = 0.0f;
        // }
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void loadSaveData(PropertySaveData saveData)
    {
        Capacity = saveData.Capacity;
        HeadCount = saveData.HeadCount;
        MaxCapacity = saveData.MaxCapacity;
        HouseLevel = saveData.HouseLevel;
    }


    public void AddPerson()
    {
        if (Capacity > HeadCount)
        {
            HeadCount++;
        }
    }
    public void IncreaseCapacity(int add)
    {
        if (Capacity + add < MaxCapacity)
            Capacity += add;
    }

}

public enum PropertyType
{
    Residental,
    Industrial,
    Shopping
}