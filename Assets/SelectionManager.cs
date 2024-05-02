using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface SelectableObject {
   
    Vector3Int GetGridPosition();
    void SetGridPosition(Vector3Int gridPosition);
    void ToggleHighlight(bool on);
    void FreezeHighlight(bool shouldFreeze);
    GameObject GetGameObject();
    string GetDescription();
}

public class SelectionManager : MonoBehaviour, SelectableObject
{
    Vector3Int? gridPosition = null;
    public string Description {get; set;} = "";
    public Vector3Int GetGridPosition()
    {
        if (this.gridPosition != null) {
            return this.gridPosition ?? new Vector3Int();
        }
        var manager = FindObjectOfType<GridManager>(); 
        var gridPosition = manager.GetGridPositionForGamePosition(transform.position);
        return gridPosition;
    }

    public void SetGridPosition(Vector3Int gridPosition) {
        this.gridPosition = gridPosition;
    }
 
    private Highlight GetHighlight()
    {
       return gameObject.GetComponent<Highlight>();
    }

    public void ToggleHighlight(bool on)
    {
        GetHighlight().ToggleHighlight(on);
    }

    public void FreezeHighlight(bool shouldFreeze) {
        GetHighlight().IsHighlightChangeable = !shouldFreeze;
    }
 
    public GameObject GetGameObject()
    {
        return gameObject;
    }
    void OnMouseEnter()
    {
        var manager = FindObjectOfType<GridManager>();   
        manager.ObjectSelectedAtPosition(GetGridPosition());
    }

    void OnMouseExit()
    {
        var manager = FindObjectOfType<GridManager>();   
        manager.ObjectDeselectedAtPosition(GetGridPosition());
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public string GetDescription()
    {
        return Description;
    }
}
