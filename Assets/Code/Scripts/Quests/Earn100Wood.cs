using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Earn100Wood")]
public class Earn100Wood : Quest
{
    public override void CheckTheQuest()
    {
        if (PlayerBalance.Wood >= 100)
        {
            questText = "Earn 100 wood quest completed!";
            isQuestIsDone = true;
        }
    }
}
