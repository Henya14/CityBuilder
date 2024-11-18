using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEditor.PlayerSettings;

public class RawMaterialManager : MonoBehaviour
{
    [SerializeField] private int narrowBy = 1000;
    [SerializeField] private List<RawMaterialWithRearity> rawMaterials;
    private Dictionary<Rect,string> rawMaterialPlaces;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public  List<RawMaterialWithRearity> GetRawMaterials(){
        if(rawMaterials == null || rawMaterials.Count == 0)
        {
            LoadRawMaterials();
        }
        return rawMaterials;
    }
    public void LoadRawMaterials()
    {
        //Beolvasas
        rawMaterials = new List<RawMaterialWithRearity>();
        rawMaterialPlaces = new Dictionary<Rect, string>();
        rawMaterials = ResourceImporter.GetRawMaterials();
        var resourceManager = FindAnyObjectByType<ResourceManager>();
        foreach (var rawMaterial in rawMaterials)
        {
            resourceManager.AddResourceAsRaw(rawMaterial);
        }

        if(rawMaterials.Count == 0) 
        {
            Debug.LogWarning("Raw Materials not found");
        }
    }
    public void AddRect(Rect rect, string type)
    {
        if(rawMaterialPlaces == null)
        {
            rawMaterialPlaces = new Dictionary<Rect, string>();
        }
        if(!rawMaterialPlaces.ContainsKey(rect))
        {
            rawMaterialPlaces.Add(rect,type);
            /*
            var sp1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var sp2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sp1.transform.position = (new Vector3(rect.xMin,0,rect.yMin));
            sp2.transform.position = (new Vector3(rect.xMax, 0, rect.yMax));
            sp1.name = "TopRes";
            sp2.name = "BottomRes";
            */
        }
    }

    public bool IsOnRawMaterial(Rect rect)
    {
        bool overlaps=false;
        foreach(var raw in rawMaterialPlaces.Keys)
        {
            if(raw.Overlaps(rect, true)){
                overlaps = true;
                break;
            }
        }
        return overlaps;
    }
    public bool IsOnRawMaterial(List<Vector2> rectCorners)
    {
        if(rectCorners.Count != 4) return false;

        foreach (var raw in rawMaterialPlaces.Keys)
        {
            //Narrowing
            var Distance = Vector2.Distance(rectCorners[0], raw.center);
            if (narrowBy < Distance) continue;

            //Search for match
            Vector2[] rawCorners = {
                new Vector2(raw.xMin, raw.yMin), new Vector2(raw.xMax, raw.yMin),
                new Vector2(raw.xMax, raw.yMax), new Vector2(raw.xMin, raw.yMax)
            };
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {

                    if (LinesIntersect(rawCorners[i], rawCorners[(j + 1) % 4], rectCorners[i], rectCorners[(i + 1) % 4]))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    public List<string> OverlapedRawMaterials(Rect rect)
    {

        var rawMaterials = new List<string>();
        foreach (var raw in rawMaterialPlaces)
        {
            if(rawMaterials.Contains(raw.Value)) continue;
            if (raw.Key.Overlaps(rect, true))
            {
                rawMaterials.Add(raw.Value);
            }
        }
        return rawMaterials;

    }
    public List<string> OverlapedRawMaterials(List<Vector2> rectCorners)
    {
        if (rectCorners.Count != 4) return null;

        var rawMaterials = new List<string>();
        foreach (var raw in rawMaterialPlaces)
        {
            //Narrowing
            var Distance = Vector2.Distance(rectCorners[0], raw.Key.center);
            if (narrowBy < Distance) continue;

            //If it's already added can be skipped
            if (rawMaterials.Contains(raw.Value)) continue;


            //Search for match
            Vector2[] rawCorners = {
                new Vector2(raw.Key.xMin, raw.Key.yMin), new Vector2(raw.Key.xMax, raw.Key.yMin),
                new Vector2(raw.Key.xMax, raw.Key.yMax), new Vector2(raw.Key.xMin, raw.Key.yMax)
            };
            bool skip = false;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {

                    if (LinesIntersect(rawCorners[i], rawCorners[(j + 1) % 4], rectCorners[i], rectCorners[(i + 1) % 4]))
                    {
                        rawMaterials.Add(raw.Value);
                        skip = true;
                        break;
                    }
                }
                if(skip) break;
            }
        }
        return rawMaterials;

    }
    /// <summary>
    /// Tells if two section (given by their endpoints) are crosses eacother
    /// </summary>
    /// <param name="a1">First section first endpoint</param>
    /// <param name="a2">First section second endpoint</param>
    /// <param name="b1">Second section first endpoint</param>
    /// <param name="b2">Second section second endpoint</param>
    /// <returns>True if they're crossing</returns>
    public static bool LinesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        float dir1 = Dir(a1, a2, b1);
        float dir2 = Dir(a1, a2, b2);
        float dir3 = Dir(b1, b2, a1);
        float dir4 = Dir(b1, b2, a2);

        // if the 2-2 end points of the vectors are to the left and to the right compared to the other vector they intersect
        if (dir1 * dir2 < 0 && dir3 * dir4 < 0)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Tells where the dot compared to the line
    /// </summary>
    /// <param name="a"> line point </param>
    /// <param name="b"> other line point </param>
    /// <param name="c"> the dot</param>
    /// <returns>
    /// If pos then left, 
    /// If neg then right, 
    /// If 0 on line
    /// </returns>
    public static float Dir(Vector2 a, Vector2 b, Vector2 c) 
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    }
}
