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

    public static Action OnPlayerBalanceChanged;

    [SerializeField] public static int Balance { get; private set; }


    void Start() {
        residentsTaxes = new List<float>();
        shopTaxes = new List<float>();
        factoryTaxes = new List<float>();

        TimeManager.OnHourChanged += calcTaxes;

    }

    private void calcTaxes() {
        updatePopulation();

        float currTaxIncome = 0;

        for (int i = 0; i < residentsTaxes.Count; i++)
            currTaxIncome += residentsTaxes[i] * residentsPopulation[i];

        for (int i = 0; i < shopTaxes.Count; i++)
            currTaxIncome += shopTaxes[i] * shopPopulation[i];

        for (int i = 0; i < factoryTaxes.Count; i++)
            currTaxIncome += factoryTaxes[i] * factoryPopulation[i];

        Balance += (int)currTaxIncome;
    }

    private void updatePopulation() {
        //Get the population numbers from a gridManager here, now its a dummy data
        for (int i = 0; i < residentsPopulation.Count; i++)
            residentsPopulation[i] += 1;

        for (int i = 0; i < shopPopulation.Count; i++)
            shopPopulation[i] += 1;

        for (int i = 0; i <  factoryPopulation.Count; i++)
            factoryPopulation[i] += 1;
    }

    //Only for load the game back from a savegame
    public void LoadTaxes(List<float> loadedRTaxes, List<float> loadedSTaxes, List<float> loadedFTaxes) {
        residentsTaxes = loadedRTaxes;
        shopTaxes = loadedSTaxes;
        factoryTaxes = loadedFTaxes;
    }

    public void increaseBalance(int amount)
    {
        if(amount >= 0)
        {
            Balance += amount;
            OnPlayerBalanceChanged?.Invoke();
        }
    }

    public void decreaseBalance(int amount)
    {
        if (amount <= 0 && ((Balance - amount) >= 0))
        {
            Balance -= amount;
            OnPlayerBalanceChanged?.Invoke();
        }  
    }
}
