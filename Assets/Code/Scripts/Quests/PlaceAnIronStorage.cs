using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PlaceAnIronStorage", menuName = "Quests/PlaceAnIronStorage")]
public class PlaceAnIronStorage : Quest
{
    public override void CheckTheQuest()
    {

        if (FindObjectsOfType<ResourceStorage>().Where(p => p.Resource.ResourceName.Contains("Iron")&& p.name.Contains("torage")).ToList().Count != 0)
        {

            questText = "Iron Storage built";
            isQuestIsDone = true;
        }
    }
    public override string GetHint()
    {
        return "Build a Storage and if necessary change it's type to Iron \n \t(click on it to see the settings)";
    }
}