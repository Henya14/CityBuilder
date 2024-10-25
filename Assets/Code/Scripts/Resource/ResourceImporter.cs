using Palmmedia.ReportGenerator.Core.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

public static class ResourceImporter
{
    [SerializeField]
    public static string resourceFolderPath = "MaterialDatas/";
    public static List<RawMaterialWithRearity> GetRawMaterials()
    {
        var raw = new List<RawMaterialWithRearity>();
        raw = Load<RawMaterialWithRearity>(resourceFolderPath + "RawMaterials.json");
        return raw;
    }
    /*
    public static List<Resource> GetResources()
    {
        var res = new List<Resource>();
        var raw = Load<ResourceRecipe>(resourceFolderPath + "ResourceRecipes.json");
        foreach ( var r in raw)
        {
            res.Add(new Resource(r.name, r.description, r.amount_per_hour, r.GetRecipe()));
        }
        return res;
    }*/
    public static void GetResources(Component component)
    {
        var raw = Load<ResourceRecipe>(resourceFolderPath + "ResourceRecipes.json");
        foreach (var r in raw)
        {
            
            var o = new GameObject(r.name);
            o.transform.parent = component.transform;
            Resource.CreateComponent(o, r.name,r.description,r.amount_per_hour,r.GetRecipe());
            //o.GetComponent<Resource>.Change(r.name, r.description, r.amount_per_hour, r.GetRecipe());
        }
    }

    private static List<T> Load<T>(string path)
    {
        var list = new List<T>();
        string json = File.ReadAllText(path);

        var serializable = JsonUtility.FromJson<SerializableList<T>>(json);

        list = serializable.list;
        return list;
    } 

    public static void Save()
    {
        var listR = new List<ResourceRecipe>() {
            new("Iron", "iron desc...", 1.5F, new Dictionary<string, float>{{"példa",1.0F}})
        };
        SerializableList<ResourceRecipe> serializableList = new SerializableList<ResourceRecipe>();
        serializableList.list = listR;
        
        string rawJson = JsonUtility.ToJson(serializableList);
        Debug.Log(rawJson);

        File.WriteAllText(resourceFolderPath + "ResourceRecipes.json", rawJson);
        /*
        var rawMaterials = new List<RawMaterialWithRearity>{
             new RawMaterialWithRearity("Iron", 0.1F,new Color(0.5490196F,0.5490196F,0.5490196F)),
             new RawMaterialWithRearity("Copper", 0.093F, new Color(0.9176471F,0.4470588F,0.1254902F)),
             new RawMaterialWithRearity("Wood", 0.01F, new Color(0.4745098F,0.2745098F,0.1411765F,0.5960785F))
        };




        SerializableList<RawMaterialWithRearity> serializableList = new SerializableList<RawMaterialWithRearity>();
        serializableList.list = rawMaterials;

        string rawJson = JsonUtility.ToJson(serializableList);
        
        Debug.Log(rawJson);
        
        File.WriteAllText(resourceFolderPath + "RawMaterials.json", rawJson);

        /*
        string tileJson = File.ReadAllText(saveFileName + "Tiles.json");
        tilesToSaveOrLoad = JsonUtility.FromJson<SerializableList<TileSaveData>>(tileJson);
        */
    }

    [Serializable]
    private class ResourceRecipe
    {
        public string name;
        public string description;
        public float amount_per_hour;
        public List<MyKeyNValue> recipe;

        public ResourceRecipe(string v1, string v2, float v3, Dictionary<string, float> dictionary)
        {
            name = v1;
            description = v2;
            amount_per_hour = v3;

            recipe = Transcribe(dictionary);

        }
        private List<MyKeyNValue> Transcribe(Dictionary<string, float> dictionary)
        {
            var list = new List<MyKeyNValue>();
            foreach(KeyValuePair<string, float> kvp in dictionary)
            {
                list.Add(new MyKeyNValue(kvp.Key, kvp.Value));
            }
            return list;
        }
        public Dictionary<string, float> GetRecipe()
        {
            var dict = new Dictionary<string, float>();
            foreach(var item in recipe)
            {
                dict.Add(item.key, item.value);
            }
            return dict;
        }
    }
    [Serializable]
    private class MyKeyNValue
    {
        public string key;
        public float value;

        public MyKeyNValue(string key, float value)
        {
            this.key = key;
            this.value = value;
        }
    }

}

