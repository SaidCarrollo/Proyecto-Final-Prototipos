using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Collider))]
public class CollisionTrigger : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("El tag que deben tener los objetos para que el script los considere 'v�lidos'.")]
    [SerializeField] private string requiredTag = "Inflamable";

    [Header("Events")]
    [Tooltip("Este evento se llama en el preciso instante en que el �LTIMO objeto con el tag requerido sale del trigger.")]
    public UnityEvent OnLastValidObjectExits;
    [SerializeField] private VignetteEvent vignetteEvent;
    private List<Collider> collidersInTrigger = new List<Collider>();

    private bool wasValidObjectPresent = false;
    [SerializeField] private BadgeManager badgeManager;

    [SerializeField] private string timelyBadgeID = "SinObjetosInflamables";
    [SerializeField] private string lateBadgeID = "ObjetosInflamablesTarde";
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameEventstring messageEvent;
    private void Awake()
    {
        var triggerCollider = GetComponent<Collider>();
        if (!triggerCollider.isTrigger)
        {
            Debug.LogError($"El Collider en '{gameObject.name}' no est� configurado como 'Is Trigger'. El script no funcionar�.", this);
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (!collidersInTrigger.Contains(other))
        {
            collidersInTrigger.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (collidersInTrigger.Contains(other))
        {
            collidersInTrigger.Remove(other);
        }
    }

    private void Update()
    {
        collidersInTrigger.RemoveAll(item => item == null);
        bool isValiObjectPresentNow = collidersInTrigger.Any(col => col.CompareTag(requiredTag));
        if (wasValidObjectPresent && !isValiObjectPresentNow)
        {
            if (gameManager == null)
            {
                Debug.LogError("GameManager no asignado en CollisionTrigger. No se puede determinar el estado del juego.");
                return;
            }

            if (gameManager.IsFireUncontrolled)
            {
                Debug.Log($"<color=red>Acci�n Tard�a:</color> El �ltimo objeto '{requiredTag}' sali�, pero el fuego ya est� descontrolado.");
                if (badgeManager != null)
                {
                    badgeManager.UnlockBadge(lateBadgeID);
                }
                if (vignetteEvent != null)
                {
                    vignetteEvent.Raise(Color.red, 0.5f, 3f);
                }
                messageEvent.Raise("Es demasiado tarde y riesgoso mover esto ahora.");
            }
            else
            {

                Debug.Log($"<color=green>Acci�n Correcta:</color> El �ltimo objeto '{requiredTag}' ha salido del trigger a tiempo.");
                if (badgeManager != null)
                {
                    badgeManager.UnlockBadge(timelyBadgeID);
                }
                if (vignetteEvent != null)
                {
                    vignetteEvent.Raise(Color.green, 0.4f, 2f);
                }
                messageEvent.Raise("Es mejor alejar estos objetos para evitar que se prendan");
            }

            OnLastValidObjectExits.Invoke();
        }

        wasValidObjectPresent = isValiObjectPresentNow;
    }
}