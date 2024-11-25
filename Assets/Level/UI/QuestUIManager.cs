using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class QuestUIManager : MonoBehaviour
{
    private Button exit;
    private VisualElement root;
    private int questcount;
    // Start is called before the first frame update
    void Start()
    {
        questcount = -1;
    }
    public void SetRoot(ref VisualElement root)
    {
        this.root = root;
    }
    public void SetExit(ref Button exit)
    {
        this.exit = exit;
        exit.style.display = DisplayStyle.None;
    }

    // Update is called once per frame
    void Update()
    {
        if (root!=null)
        {
            List<Quest> list = FindObjectOfType<PlayerBalance>().quests;
            if (questcount != list.Count)
            {
                root.Clear();
                questcount = list.Count;
                if(questcount==0) { 
                    root.GetFirstAncestorOfType<ListView>().style.display = DisplayStyle.None; 
                    exit.style.display = DisplayStyle.Flex;
                }
                for(int i = 0; i < questcount; i++)
                {
                    Quest quest = list[i];
                    Label label = new Label();
                    label.text = quest.GetHint();
                    root.Add(label);
                }

            }
        }

    }
}
