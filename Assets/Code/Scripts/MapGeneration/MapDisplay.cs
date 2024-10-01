using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class MapDisplay : MonoBehaviour
{
    [SerializeField] Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
  
    private MeshCollider meshCollider;
    private Highlight highlight;

    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
       meshFilter.sharedMesh = meshData.CreateMesh();
       meshRenderer.sharedMaterial.mainTexture = texture;
       foreach (var component in meshRenderer.gameObject.GetComponents<Component>()) {
        if (component is MeshCollider) {
            DestroyImmediate(component);
        } else if(component is Highlight) {
            DestroyImmediate(component);
        }
       }
       highlight = meshRenderer.gameObject.GetOrAddComponent<Highlight>();
       meshCollider = meshRenderer.gameObject.AddComponent<MeshCollider>();
    }
}
