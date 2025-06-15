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
            if (badgeManager != null)
            {
                badgeManager.UnlockBadge("SInObjectosinflamables");
            }
            OnLastValidObjectExits.Invoke();
            vignetteEvent.Raise(Color.green, 0.4f, 2f);
            Debug.Log($"<color=green>Evento Disparado:</color> El �ltimo objeto con el tag '{requiredTag}' ha salido del trigger.");
        }

        wasValidObjectPresent = isValiObjectPresentNow;
    }
}