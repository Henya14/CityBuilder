using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MakeWireOrBirck", menuName = "Quests/MakeWireOrBirck")]

public class MakeWireOrBirck : Quest
{
    public override void CheckTheQuest()
    {

        if (PlayerBalance.instance.GetResourceManager().FindResourceByName("Wire").GetAmount() > 0 || PlayerBalance.instance.GetResourceManager().FindResourceByName("Brick").GetAmount() > 0)
        {

            questText = "Made Wire Or Birck";
            isQuestIsDone = true;
        }
    }
    public override string GetHint()
    {
        return $"Make some Bricks or wire" +
            $"\n\tSome resources dont have special buildings " +
            $"\n\tto make them place a producer and select the wanted one " +
            $"\n\t(and delivered the needed resource(s))";
    }
}