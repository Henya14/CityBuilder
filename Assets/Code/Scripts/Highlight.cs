using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlight : MonoBehaviour
{
    [SerializeField] private List<Renderer> renderers;
    private List<Material> materials;
    [SerializeField] private Color highlightColor = Color.white;
    
    private void Awake() {
        materials = new List<Material>();
        foreach(var renderer in renderers) {
            materials.AddRange(new List<Material>(renderer.materials));
        }
    }

    public void ToggleHighlight(bool on) {
        if (on) {
            foreach (var material in materials) {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", highlightColor);
            }
        } else {
            foreach (var material in materials) {
                material.DisableKeyword("_EMISSION");
            }
        }
    }
}
