using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/TutorialCompleted")]

public class TutorialCompleted : Quest
{
    public override void CheckTheQuest()
    {
        var resman = PlayerBalance.instance.GetResourceManager();
        if (PlayerBalance.instance.quests.Count == 1)
        {

            questText = "Tutorial Completed!\n You can exit now...";
            isQuestIsDone = true;
        }
    }
    public override string GetHint()
    {
        return "Complete all quest";
    }
}