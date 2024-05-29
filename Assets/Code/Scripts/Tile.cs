using UnityEngine;



public class Tile : MonoBehaviour
{
    public Vector3Int gridPosition { get; set; }

    [SerializeField] public string Name;
    [SerializeField] public string Description;
    public Vector2Int tileSize { get; set; } = new Vector2Int(1, 1);

    public Morality tileMorality { get; set; }

    [SerializeField] Material destMaterial;
    Material test;
    public Material baseMaterial { get; set; }
    private void Start()
    {
        baseMaterial = GetComponent<MeshRenderer>().material;
        test = new Material(destMaterial);
        var selectionManager = gameObject.AddComponent<SelectionManager>();
        selectionManager.Init(gridPosition, Description, SelectableObjectType.Tile);
    }

    public void changeMaterial()
    {
        Color customColor = new Color(0.1f, 0.9f * tileMorality.moralityLevel, 0.7f, 1.0f);
        test.SetColor("_Color", customColor);
        GetComponent<MeshRenderer>().material = test;
    }

    public void resetMaterial()
    {
        GetComponent<MeshRenderer>().material = baseMaterial;
    }


    public string GetDescription()
    {
        return Description;
    }
}
