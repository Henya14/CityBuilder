using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public class Tile : MonoBehaviour
{
    public Vector3Int gridPosition {get; set;}

    [SerializeField] public string description;
    public Vector2Int tileSize {get; set;} = new Vector2Int(1,1); 

    public Morality tileMorality { get; set;}

    [SerializeField] Material destMaterial;
    private Color baseHighlightColor;
    private Highlight highlight;
    private void Start()
    {
        highlight = GetComponent<Highlight>();
        baseHighlightColor = highlight.GetHighlightColor();
    }

    public void changeMaterial()
    {
        Color customColor = new Color(0.1f, 0.9f * tileMorality.moralityLevel, 0.7f, 1.0f);
        Debug.Log(customColor);
        highlight.SetHighlightColor(customColor);
        highlight.ToggleHighlight(true);
    }

    public void resetMaterial()
    {
        highlight.ToggleHighlight(false);
        highlight.SetHighlightColor(baseHighlightColor);
    }
}
