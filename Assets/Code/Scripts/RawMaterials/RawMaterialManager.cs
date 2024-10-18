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
        rawMaterials = new List<RawMaterialWithRearity>();
        rawMaterials = ResourceImporter.GetRawMaterials();
        /*rawMaterials = new List<RawMaterialWithRearity>{
             new RawMaterialWithRearity("Iron", 0.1F,new Color(0.5490196F,0.5490196F,0.5490196F)),
             new RawMaterialWithRearity("Copper", 0.093F, new Color(0.9176471F,0.4470588F,0.1254902F)),
             new RawMaterialWithRearity("Wood", 0.01F, new Color(0.4745098F,0.2745098F,0.1411765F,0.5960785F))
        };*/
        
    }
}
