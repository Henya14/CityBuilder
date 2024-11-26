using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "TransportToStorage", menuName = "Quests/TransportToStorage")]
public class TransportToStorage : Quest
{
    public override void CheckTheQuest()
    {

        if (FindObjectsOfType<ResourceStorage>().Where(p => p.name.Contains("torage") && p.StoredAmount>0).ToList().Count != 0)
        {

            questText = "Resource transported to storage";
            isQuestIsDone = true;
        }
    }
    public override string GetHint()
    {
        return "Transport resource to storage \n" +
            "\t(click on a producer and add a new Route)\n" +
            "\tWhen creating a route you can select beside the destination,\n" +
            "\t\tand the wanted carrire type, the amount \n" +
            "\t\tand whether it repeats every hour as well";
    }
}