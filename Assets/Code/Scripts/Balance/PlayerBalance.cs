using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBalance : MonoBehaviour
{
    [SerializeField] int balance = 0;
    
    List<float> residentsTaxes;
    List<float> shopTaxes;
    List<float> factoryTaxes;

    List<float> residentsPopulation;
    List<float> shopPopulation;
    List<float> factoryPopulation;


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

        balance += (int)currTaxIncome;
    }

    private void updatePopulation() {
        //Get the population numbers from a manager here, now its a dummy data
        for (int i = 0; residentsPopulation.Count > 0; i++)
            residentsPopulation[i] += 1;

        for (int i = 0; shopPopulation.Count > 0; i++)
            shopPopulation[i] += 1;

        for (int i = 0; factoryPopulation.Count > 0; i++)
            factoryPopulation[i] += 1;
    }

    //Only for load the game back from a savegame
    public void LoadTaxes(List<float> loadedRTaxes, List<float> loadedSTaxes, List<float> loadedFTaxes) {
        residentsTaxes = loadedRTaxes;
        shopTaxes = loadedSTaxes;
        factoryTaxes = loadedFTaxes;
    }
}
