using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/PlaceARoad")]
public class PlaceARoad : Quest
{

    public override void CheckTheQuest()
    {
        if (FindObjectOfType<RoadMesh>() != null)
        {
            questText = "Place a road completed!";
            isQuestIsDone = true;
        }
    }
    public override string GetHint()
    {
        return "Place a road to build buildings next to it (use build menu)";
    }
}
