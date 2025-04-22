using UnityEngine;
using System.Collections;

public class GrabbableObject : MonoBehaviour
{
    [Header("Water Effect")]
    [SerializeField] private Color wetColor = new Color(0.639f, 0.905f, 1f); // #A3E7FF
    [SerializeField] private float wetDuration = 20f; // 20 segundos de efecto mojado

    private Renderer objectRenderer;
    private Color originalColor;
    private bool estaMojado = false;
    private Coroutine dryingCoroutine;

    public bool EstaMojado => estaMojado;

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
    }

    public void SetWet(bool wet)
    {
        // Si ya está en el estado que queremos, no hacer nada
        if (estaMojado == wet) return;

        estaMojado = wet;

        if (objectRenderer == null) return;

        // Solo cambiar el color si está mojado
        if (wet)
        {
            objectRenderer.material.color = wetColor;
            // Iniciar temporizador para secado
            if (dryingCoroutine != null) StopCoroutine(dryingCoroutine);
            dryingCoroutine = StartCoroutine(DryAfterTime());
        }
        //  No restaurar el color aquí, solo cuando se cumplan los 20 segundos
    }

    private IEnumerator DryAfterTime()
    {
        yield return new WaitForSeconds(wetDuration);
        SecarObjeto(); // Método separado para limpieza
    }

    private void SecarObjeto()
    {
        estaMojado = false;
        if (objectRenderer != null)
        {
            objectRenderer.material.color = originalColor;
        }
        if (dryingCoroutine != null)
        {
            StopCoroutine(dryingCoroutine);
            dryingCoroutine = null;
        }
    }
}