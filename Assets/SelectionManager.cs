using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface SelectableObject {
   
    Vector3Int GetGridPosition();
    void ToggleHighlight(bool on);
    GameObject GetGameObject();
}

public class SelectionManager : MonoBehaviour, SelectableObject
{
    public Vector3Int GetGridPosition()
    {
        var manager = FindObjectOfType<GridManager>(); 
        var gridPosition = manager.GetGridPositionForGamePosition(transform.position);
        return gridPosition;
    }

    private Highlight GetHighlight()
    {
        return gameObject.GetComponent<Highlight>();
    }

    public void ToggleHighlight(bool on)
    {
        GetHighlight().ToggleHighlight(on);
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
}
