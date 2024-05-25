using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Quest : ScriptableObject
{
    protected bool isQuestIsDone = false;
    protected string questText;
    public virtual void CheckTheQuest()
    {
        //Override in every implementation
    }

    public bool IsDone()
    {
        if (isQuestIsDone)
        {
            GameUIManager gameUIManager = FindObjectOfType<GameUIManager>();
            gameUIManager.QuestCompleted(questText);    
        }
            
        return isQuestIsDone;
    }
}
