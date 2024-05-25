using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/EarnMillionDollar")]
public class EarnMillionDollar : Quest
{
    public override void CheckTheQuest()
    {
        if (PlayerBalance.Balance >= 10)
        {
            questText = "Game completed!";
            isQuestIsDone = true;
        }
    }
}
