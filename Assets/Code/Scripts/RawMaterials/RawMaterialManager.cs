using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RawMaterialManager : MonoBehaviour
{
    public static List<RawMaterialWithRearity> rawMaterials;
    [SerializeField] public MapGenarator mapGenarator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public static List<RawMaterialWithRearity> GetRawMaterials(){
        if(rawMaterials == null)
        {
            LoadRawMaterials();
        }
        return rawMaterials;
    }
    public static void LoadRawMaterials()
    {
        //Beolvasas
        rawMaterials = new List<RawMaterialWithRearity>{
             new RawMaterialWithRearity("Iron", 0.01F, new Color(140,140,140)),
             new RawMaterialWithRearity("Copper", 0.005F, new Color(234,114,32)),
             new RawMaterialWithRearity("Wood", 0.1F, new Color(121,70,36,152))
        };
        
    }
}
