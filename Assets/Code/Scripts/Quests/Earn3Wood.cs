using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Earn3Wood")]

public class Earn3Wood : Quest
{
    public override void CheckTheQuest()
    {
        var resman = PlayerBalance.instance.GetResourceManager();
        if (resman != null)
        {
            var woodres = resman.FindResourceByName("Wood");
            if (woodres != null)
            {
                if(woodres.GetAmount() > 3) {

                    questText = "Earn 3 wood quest completed!";
                    isQuestIsDone = true;
                }
            }
        }
    }
}
