using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBalance : MonoBehaviour
{
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

        Balance += (int)currTaxIncome;
    }

    private void UpdatePopulation() {
        //Get the population numbers from a gridManager here, now its a dummy data
        for (int i = 0; i < residentsPopulation.Count; i++)
            residentsPopulation[i] += 1;

        for (int i = 0; i < shopPopulation.Count; i++)
            shopPopulation[i] += 1;

        for (int i = 0; i <  factoryPopulation.Count; i++)
            factoryPopulation[i] += 1;
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
                if (shopTaxes[level] >= 1)
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
}
