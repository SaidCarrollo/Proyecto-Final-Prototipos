using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class InteractableVisuals : MonoBehaviour
{
    [Header("Configuración del Shader")]
    [Tooltip("El nombre exacto de la propiedad en tu Shader.")]
    private string widthProperty = "_OutlineWidth";
    private string colorProperty = "_OutlineColor";

    [Header("Estado Idle (Titileo)")]
    public Color colorIdle = Color.white;
    public float anchoMinimo = 0.001f;
    public float anchoMaximo = 0.0025f;
    public float velocidadTitileo = 2f;

    [Header("Estado Hover (Enfocado)")]
    public Color colorHover = Color.yellow; // Un dorado o amarillo destaca bien
    public float anchoHover = 0.006f;       // Bastante más grueso
    public float velocidadTransicion = 10f; // Qué tan rápido crece la línea

    // Variables internas
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;
    private bool _isHovered = false;
    private float _targetWidth;
    private float _currentWidth;
    private Color _currentColor;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        // Iniciar valores
        _currentWidth = anchoMinimo;
        _currentColor = colorIdle;
    }

    void Update()
    {
        if (_isHovered)
        {
            // Lógica HOVER: Ir hacia el ancho máximo y color fijo
            _targetWidth = anchoHover;
            _currentColor = Color.Lerp(_currentColor, colorHover, Time.deltaTime * velocidadTransicion);
        }
        else
        {
            // Lógica IDLE: Titileo suave (Onda Senoidal)
            // Mathf.PingPong o Sin funcionan bien. Usaremos Sin para suavidad.
            float t = (Mathf.Sin(Time.time * velocidadTitileo) + 1f) / 2f; // Valor entre 0 y 1
            _targetWidth = Mathf.Lerp(anchoMinimo, anchoMaximo, t);

            // Volver al color base suavemente
            _currentColor = Color.Lerp(_currentColor, colorIdle, Time.deltaTime * velocidadTransicion);
        }

        // Suavizado del cambio de ancho (Lerp)
        _currentWidth = Mathf.Lerp(_currentWidth, _targetWidth, Time.deltaTime * velocidadTransicion);

        // APLICAR AL SHADER
        // 1. Obtenemos el bloque actual
        _renderer.GetPropertyBlock(_propBlock);

        // 2. Modificamos valores
        _propBlock.SetFloat(widthProperty, _currentWidth);
        _propBlock.SetColor(colorProperty, _currentColor);

        // 3. Enviamos de vuelta (Esto es MUY eficiente, no crea nuevos materiales)
        _renderer.SetPropertyBlock(_propBlock);
    }

    // --- Métodos públicos para llamar desde PlayerInteraction ---
    public void OnFocusEnter()
    {
        _isHovered = true;
    }

    public void OnFocusExit()
    {
        _isHovered = false;
    }
}