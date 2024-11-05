using UnityEngine;
using UnityEngine.Video;



public class Tile : MonoBehaviour
{
    public Vector3Int gridPosition { get; set; }

    [SerializeField] public string Name;
    [SerializeField] public string Description;
    public Vector2Int tileSize { get; set; } = new Vector2Int(1, 1);

    public Morality tileMorality { get; set; }

    Material test;
    public Material baseMaterial { get; set; }
    private void Start()
    {
        baseMaterial = GetComponentInChildren<MeshRenderer>().material;
        
        var selectionManager = gameObject.AddComponent<SelectionManager>();
        //selectionManager.Init(gridPosition, Description, SelectableObjectType.Tile);
    }

    public void SetMoralityMaterial(Material destMaterial) {
        test = new Material(destMaterial);
    }

    public void changeMaterial()
    {
        Color customColor = new Color(0.1f, 0.9f * tileMorality.moralityLevel, 0.7f, 1.0f);
        test.SetColor("_Color", customColor);
        GetComponentInChildren<MeshRenderer>().material = test;
    }

    public void resetMaterial()
    {
        GetComponentInChildren<MeshRenderer>().material = baseMaterial;
    }


    public string GetDescription()
    {
        return Description;
    }
}
