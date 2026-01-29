using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening; // No olvides importar la librería de DOTween

public class BotonAnimado : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float escalaHover = 1.1f;
    [SerializeField] private float duracion = 0.2f;

    private Vector3 escalaOriginal;
    private Button btn;

    private void Awake()
    {
        escalaOriginal = transform.localScale; // OJO: Esto asume que el prefab está en escala 1,1,1
        btn = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Seguridad extra: Si el botón no es interactuable (controlado por QuizManager),
        // no hacemos la animación de hover.
        if (btn != null && !btn.interactable) return;

        transform.DOKill(true); // "true" completa la animación anterior instantáneamente

        // Usamos escalaOriginal * escalaHover para asegurar consistencia
        // Pero si QuizManager cambió la escala base, esto podría dar saltos.
        // Dado que bloqueamos el raycast en QuizManager, aquí estamos seguros.
        transform.DOScale(escalaOriginal * escalaHover, duracion)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill(true);

        transform.DOScale(escalaOriginal, duracion)
            .SetUpdate(true);
    }

    // Método de utilidad por si necesitas forzar el reset desde fuera
    public void ResetearEscala()
    {
        transform.localScale = escalaOriginal;
    }
    public void QuitApplication()
    {
        Application.Quit();
    }
}