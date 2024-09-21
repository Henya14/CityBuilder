using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoadMesh : MonoBehaviour
{
    [SerializeField] Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
  
    private MeshCollider meshCollider;
    private Highlight highlight;

    void Start() {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData, Material material)//, Texture2D texture)
    {
       var meshFilter = GetComponent<MeshFilter>();
       var meshRenderer = GetComponent<MeshRenderer>();
       meshFilter.sharedMesh = meshData.CreateMesh();
       meshRenderer.material = material;
       var position = meshRenderer.transform.position;
       position.y = position.y + 0.1f;
       meshRenderer.transform.position = position; 
       //meshRenderer.sharedMaterial.mainTexture = texture;
       /*foreach (var component in meshRenderer.gameObject.GetComponents<Component>()) {
        if (component is MeshCollider) {
            DestroyImmediate(component);
        } else if(component is Highlight) {
            DestroyImmediate(component);
        }
       }*/
       //highlight = meshRenderer.gameObject.GetOrAddComponent<Highlight>();
       //meshCollider = meshRenderer.gameObject.AddComponent<MeshCollider>();
    }
}
