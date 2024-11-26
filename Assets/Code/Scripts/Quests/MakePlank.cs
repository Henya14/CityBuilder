using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "MakePlank", menuName = "Quests/MakePlank")]

public class MakePlank : Quest
{
    public override void CheckTheQuest()
    {

        if (PlayerBalance.instance.GetResourceManager().FindResourceByName("Plank").GetAmount()>0)
        {

            questText = "Made Plank";
            isQuestIsDone = true;
        }
    }
    public override string GetHint()
    {
        return $"Place a Sawmill and deliver {PlayerBalance.instance.GetResourceManager().FindResourceByName("Plank").GetRecipe()["Wood"]} Wood to it\n" +
            $"\t to make planks";
    }
}