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
  

    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
       meshFilter.sharedMesh = meshData.CreateMesh();
       meshRenderer.sharedMaterial.mainTexture = texture;
       meshRenderer.gameObject.GetOrAddComponent<Highlight>();
       Destroy(meshRenderer.gameObject.GetComponent<MeshCollider>());
       meshRenderer.gameObject.AddComponent<Highlight>();
       meshRenderer.gameObject.AddComponent<MeshCollider>();
    }
}
