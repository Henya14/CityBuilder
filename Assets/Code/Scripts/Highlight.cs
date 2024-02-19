using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlight : MonoBehaviour
{

    [SerializeField] Material highlightMaterial;
    [SerializeField] Material originalMaterial;
    
    private void Awake() {
        originalMaterial = gameObject.GetComponent<MeshRenderer>()?.material;
    }

    public void ToggleHighlight(bool on) {
        gameObject.GetComponent<MeshRenderer>().material = on? highlightMaterial : originalMaterial;
     }
}
