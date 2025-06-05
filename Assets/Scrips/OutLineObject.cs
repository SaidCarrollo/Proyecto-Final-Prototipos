using UnityEngine;
using UnityEngine.Rendering.Universal; 

public class OutlineObject : MonoBehaviour
{
    private Material originalMaterial;
    [SerializeField] private Material outlineMaterial;
    private Renderer objectRenderer;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        originalMaterial = objectRenderer.material;
    }

    public void EnableOutline()
    {
        objectRenderer.material = outlineMaterial;
    }

    public void DisableOutline()
    {
        objectRenderer.material = originalMaterial;
    }
}