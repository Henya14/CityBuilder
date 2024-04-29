using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlight : MonoBehaviour
{
    [SerializeField] private List<Renderer> renderers;
    private List<Material> materials;
    [SerializeField] private Color highlightColor = Color.white;
    [SerializeField] private bool isHighlighted = false;

    private void Awake()
    {
        if (renderers != null) {
            SetRenderers(renderers);
        }
    }

    public void SetRenderers(List<Renderer> renderers)
    {
        this.renderers = renderers;
        materials = new List<Material>();
        foreach (var renderer in renderers)
        {
            materials.AddRange(new List<Material>(renderer.materials));
        }
        ToggleHighlight(isHighlighted);
    }

    public void SetHighlightColor(Color color)
    {
        highlightColor = color;
        RefreshHighlight();
    }

    public Color GetHighlightColor()
    {
       return highlightColor;
    }

    public void ToggleHighlight(bool on)
    {
        isHighlighted = on;
        RefreshHighlight();
    }

    private void RefreshHighlight()
    {
        if (isHighlighted)
        {
            foreach (var material in materials)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", highlightColor);
            }
        }
        else
        {
            foreach (var material in materials)
            {
                material.DisableKeyword("_EMISSION");
            }
        }
    }
}
