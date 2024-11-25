using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Quest : ScriptableObject, HintOrTip
{
    protected bool isQuestIsDone = false;
    public void Reset()
    {
        isQuestIsDone = false;
    }
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
    //Use at loading quest
    public void QuestAlreadyDone()
    {
        isQuestIsDone=true;
    }

    public virtual string GetHint()
    {
        throw new System.NotImplementedException();
    }
}
