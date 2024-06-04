using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum SelectableObjectType {
    Road,
    Zone,
    Building,
    ZoneBuilding,
    Tile
}
public interface SelectableObject {
   
    Vector3Int GetGridPosition();
    Vector3Int GetRelativeClosestGridPosition(Vector3Int reference);
    void SetGridPosition(Vector3Int gridPosition);
    void SetGridPositions(List<Vector3Int> gridPositions);
    void ToggleHighlight(bool on);
    void FreezeHighlight(bool shouldFreeze);
    GameObject GetGameObject();
    string GetDescription();
    void SetDescription(string description);
    SelectableObjectType GetSelectableObjectType();
}

public class SelectionManager : MonoBehaviour, SelectableObject
{
    Vector3Int? gridPosition = null;
    List<Vector3Int> gridPositions = new List<Vector3Int>();
    private string Description {get; set;} = "";

    
    SelectableObjectType type;

    public void Init(Vector3Int gridPosition, string description, SelectableObjectType type) 
    {
        this.gridPosition = gridPosition;
        Description = description;
        this.type = type;
    }


    public Vector3Int GetGridPosition()
    {
        if (this.gridPosition != null) {
            return this.gridPosition ?? new Vector3Int();
        }
        var manager = FindObjectOfType<GridManager>(); 
        var gridPosition = manager.GetGridPositionForGamePosition(transform.position);
        return gridPosition;
    }

    public Vector3Int GetRelativeClosestGridPosition(Vector3Int reference) {
        float minDistance = int.MaxValue;
        Vector3Int minDistancePosition = gridPosition ?? new Vector3Int();
        foreach (var position in gridPositions) {
            var distance = (position - reference).magnitude;
            if(distance < minDistance) {
                minDistance = distance;
                minDistancePosition = position;
            }
        }
        return minDistancePosition;
    }

    public void SetGridPosition(Vector3Int gridPosition) {
        this.gridPosition = gridPosition;
    }

    public void SetGridPositions(List<Vector3Int> gridPositions) {
        this.gridPositions = gridPositions;
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

    public void SetDescription(string description)
    {
        Description = description;
    }

    public SelectableObjectType GetSelectableObjectType()
    {
        return type;
    }
}
