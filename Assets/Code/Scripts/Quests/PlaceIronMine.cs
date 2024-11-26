using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/PlaceIronMine")]

public class PlaceIronMine : Quest
{
    public override void CheckTheQuest()
    {

        if (FindObjectsOfType<ResourceProducer>().Where(p => p.Resource.ResourceName.Contains("Iron")).ToList().Count!=0)
        {

            questText = "Iron Mine built";
            isQuestIsDone = true;
        }
    }
    public override string GetHint()
    {
        return "Build a Mine on an Iron Deposit (gray square on map)";
    }
}