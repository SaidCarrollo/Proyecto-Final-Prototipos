using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShelterZone : MonoBehaviour
{
    [Header("Refs (puedes arrastrarlos en el Inspector)")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private BadgeManager badgeManager;

    [Header("Ajustes")]
    [Tooltip("ID del badge correcto por resguardarse. Debe existir en tu BadgeManager.")]
    [SerializeField] private string shelterBadgeId = "Resguardado";

    [Tooltip("Segundos continuos agachado dentro de la zona para validar el resguardo.")]
    [SerializeField] private float requiredCrouchSeconds = 4f;

    [Tooltip("Si se activa una vez, no vuelve a disparar.")]
    [SerializeField] private bool oneShot = true;

    [Header("Mensajes (opcional)")]
    [SerializeField, TextArea(1, 3)] private string enterHint = "Resguárdate aquí: agáchate y mantente así…";
    [SerializeField, TextArea(1, 3)] private string successMsg = "¡Resguardado correctamente!";

    private FirstPersonController _player;
    private bool _inside;
    private bool _completed;
    private float _crouchTimer;

    private void Reset()
    {
        // Asegura Trigger
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void Awake()
    {
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        // BadgeManager es ScriptableObject: suele asignarse manualmente en el Inspector.
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_completed && oneShot) return;

        var fpc = other.GetComponentInParent<FirstPersonController>();
        if (fpc == null) return;

        _player = fpc;
        _inside = true;
        _crouchTimer = 0f;

        if (!string.IsNullOrEmpty(enterHint) && uiManager != null)
            uiManager.OnMessageEventRaised(enterHint);
    }

    private void OnTriggerExit(Collider other)
    {
        var fpc = other.GetComponentInParent<FirstPersonController>();
        if (fpc != null && fpc == _player)
        {
            _inside = false;
            _player = null;
            _crouchTimer = 0f;
        }
    }

    private void Update()
    {
        if (!_inside || _completed || _player == null) return;

        // Requiere mantenerse agachado de forma continua
        if (_player.IsCrouching)
        {
            _crouchTimer += Time.deltaTime;

            if (_crouchTimer >= requiredCrouchSeconds)
            {
                // 1) Desbloquear badge correcto
                if (badgeManager != null && !string.IsNullOrEmpty(shelterBadgeId))
                    badgeManager.UnlockBadge(shelterBadgeId);

                // 2) Mensaje positivo
                if (uiManager != null && !string.IsNullOrEmpty(successMsg))
                    uiManager.OnMessageEventRaised(successMsg);

                // 3) Terminar nivel como VICTORIA
                if (gameManager != null)
                    gameManager.HandlePlayerSurvival();

                _completed = true;
                _inside = false;
            }
        }
        else
        {
            // Si se levantó, reinicia el conteo
            _crouchTimer = 0f;
        }
    }
}
