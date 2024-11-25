using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBalance : MonoBehaviour
{
    [SerializeField]
    ResourceManager resourcemanager;
    public static PlayerBalance instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    
    List<float> residentsTaxes = new List<float> { 0, 0, 0 };
    List<float> shopTaxes = new List<float> { 0, 0, 0 };
    List<float> factoryTaxes = new List<float> { 0, 0, 0 };

    List<float> residentsPopulation = new List<float> { 0, 0, 0 };
    List<float> shopPopulation = new List<float> { 0, 0, 0 };
    List<float> factoryPopulation = new List<float> { 0, 0, 0 };

    public List<float> ResidentsTaxes
    {
        get { return residentsTaxes; }
    }
    public List<float> ShopTaxes
    {
        get { return shopTaxes; }
    }
    public List<float> FactoryTaxes
    {
        get { return factoryTaxes; }
    }

    public static Action OnPlayerStatsChanged;

    static int balance; 
    static int wood;
    static int electricity;
    static int coal;

    [SerializeField] List<Quest> quests;
    [SerializeField] List<Quest> doneQuests;

    [SerializeField] public static int Balance { 
        get { return balance; } 
        set {
            if (value > 0)
            {
                balance += value;
                OnPlayerStatsChanged?.Invoke();
            }else if(value < 0 && ((balance - value) >= 0))
            {
                balance -= value;
                OnPlayerStatsChanged?.Invoke();
            }
        } 
    }
    [SerializeField]
    public static int Coal
    {
        get { return coal; }
        set
        {
            if (value > 0)
            {
                coal += value;
                OnPlayerStatsChanged?.Invoke();
            }
            else if (value < 0 && ((Coal - value) >= 0))
            {
                coal -= value;
                OnPlayerStatsChanged?.Invoke();
            }
        }
    }

    [SerializeField]
    public static int Electricity
    {
        get { return electricity; }
        set
        {
            if (value > 0)
            {
                electricity += value;
                OnPlayerStatsChanged?.Invoke();
            }
            else if (value < 0 && ((electricity - value) >= 0))
            {
                electricity -= value;
                OnPlayerStatsChanged?.Invoke();
            }
        }
    }

    [SerializeField]
    public static int Wood
    {
        get { return wood; }
        set
        {
            if (value > 0)
            {
                wood += value;
                OnPlayerStatsChanged?.Invoke();
            }
            else if (value < 0 && ((wood - value) >= 0))
            {
                wood -= value;
                OnPlayerStatsChanged?.Invoke();
            }
        }
    }
    public void LoadData(PlayerSaveData data)
    {
        balance = data.Balance;
        coal = data.Coal;
        electricity = data.Eletricty;
        wood = data.Wood;
        LoadTaxes(data.RresidentsTaxes.list, data.ShopTaxes.list, data.FactoryTaxes.list);

        //TODO: done quest search not with name
        var dQuests = quests.FindAll(q => data.DoneQuestsTexts.list.Contains(q.name));
        foreach (var quest in dQuests)
        {
            quest.QuestAlreadyDone();

            doneQuests.Add(quest);
            quests.Remove(quest);
        }
    }
    public List<float> GetResidnetTaxes() { return residentsTaxes; }
    public List<float> GetShopTaxes() { return shopTaxes; }
    public List<float> GetFactoryTaxes() { return factoryTaxes; }

    public List<string> GetDoneQuests() { return doneQuests.ConvertAll(x => x.name); }

    void Start() {

        TimeManager.OnHourChanged += CalcTaxes;
        TimeManager.OnHourChanged += CheckQuests;
    }

    private void CalcTaxes() {
        UpdatePopulation();

        float currTaxIncome = 0;

        for (int i = 0; i < residentsTaxes.Count; i++)
            currTaxIncome += residentsTaxes[i] * residentsPopulation[i];

        for (int i = 0; i < shopTaxes.Count; i++)
            currTaxIncome += shopTaxes[i] * shopPopulation[i];

        for (int i = 0; i < factoryTaxes.Count; i++)
            currTaxIncome += factoryTaxes[i] * factoryPopulation[i];

        Balance = (int)currTaxIncome;

        Debug.Log("Taxes: ");
        Debug.Log($"{residentsTaxes[0]} {residentsTaxes[1]} {residentsTaxes[2]}");
        Debug.Log($"{shopTaxes[0]} {shopTaxes[1]} {shopTaxes[2]}");
        Debug.Log($"{factoryTaxes[0]} {factoryTaxes[1]}   {factoryTaxes[2]}");
    }

    private void UpdatePopulation() {
        //Get the population numbers from a gridManager here, now its a dummy data

        residentsPopulation = new List<float> { 0, 0, 0 };
        shopPopulation = new List<float> { 0, 0, 0 };
        factoryPopulation = new List<float> { 0, 0, 0 };
        var gridManager= FindObjectOfType<GridManager>();
        foreach(var property in gridManager.GetProperties())
        {
            switch(property.PropertyType)
            {
                case PropertyType.Residental:
                    residentsPopulation[(int)property.HouseLevel - 1] += property.HeadCount;
                    break;
                case PropertyType.Shopping:
                    shopPopulation[(int)property.HouseLevel - 1] += property.HeadCount;
                    break;
                case PropertyType.Industrial:
                    factoryPopulation[(int)property.HouseLevel - 1] += property.HeadCount;
                    break;
            }
        }
        Debug.Log("Popolation: ");
        Debug.Log($"{residentsPopulation[0]} {residentsPopulation[1]} {residentsPopulation[2]}");
        Debug.Log($"{shopPopulation[0]} {shopPopulation[1]} {shopPopulation[2]}");
        Debug.Log($"{factoryPopulation[0]} {factoryPopulation[1]} {factoryPopulation[2]}");
    }

    public void IncreaseTaxes(string taxType, int level)
    {
        switch (taxType)
        {
            case "resident":
                residentsTaxes[level]++;
                break;
            case "shop":
                shopTaxes[level]++;
                break;
            case "factory":
                factoryTaxes[level]++;
                break;
            default: break;
        }
    }

    public void DecreaseTaxes(string taxType, int level)
    {
        switch (taxType)
        {
            case "resident":
                if(residentsTaxes[level]>=1)
                    residentsTaxes[level]--;
                break;
            case "shop":
                if (shopTaxes[level] >= 1)
                    shopTaxes[level]--;
                break;
            case "factory":
                if (factoryTaxes[level] >= 1)
                    factoryTaxes[level]--;
                break;
            default: break;
        }

    }

    //Only for load the game back from a savegame
    public void LoadTaxes(List<float> loadedRTaxes, List<float> loadedSTaxes, List<float> loadedFTaxes) {
        residentsTaxes = loadedRTaxes;
        shopTaxes = loadedSTaxes;
        factoryTaxes = loadedFTaxes;
    }

    private void CheckQuests()
    {
        for (int i = 0; i<quests.Count; i++)
        {
            quests[i].CheckTheQuest();
            if (quests[i].IsDone())
            {
                doneQuests.Add(quests[i]);
                quests.RemoveAt(i);
            }
        }
    }
    public ResourceManager GetResourceManager()
    {
        return resourcemanager;
    }
}
