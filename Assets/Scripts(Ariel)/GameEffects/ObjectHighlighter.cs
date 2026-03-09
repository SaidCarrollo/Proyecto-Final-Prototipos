using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ObjectHighlighter : MonoBehaviour
{
    [Header("Configuración del Shader")]
    private string widthProperty = "_OutlineWidth";
    private string colorProperty = "_OutlineColor";

    [Header("Estado Idle (Titileo suave)")]
    public Color colorIdle = new Color(1f, 1f, 1f, 1f);
    public float anchoMinimo = 0.001f;
    public float anchoMaximo = 0.0025f;
    public float velocidadTitileo = 2f;

    [Header("Estado Focus (Cuando lo miras)")]
    public Color colorFocus = new Color(1f, 0.92f, 0.016f, 1f);
    public float anchoFocus = 0.006f;
    public float velocidadTransicion = 10f;

    private bool isFocused = false;
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;
    private float _currentWidth;
    private Color _currentColor;
    private float _targetWidth;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        _currentWidth = anchoMinimo;
        _currentColor = colorIdle;
    }

    void Update()
    {
        if (isFocused)
        {
            _targetWidth = anchoFocus;
            _currentColor = Color.Lerp(_currentColor, colorFocus, Time.deltaTime * velocidadTransicion);
        }
        else
        {
            float t = (Mathf.Sin(Time.time * velocidadTitileo) + 1f) / 2f;
            _targetWidth = Mathf.Lerp(anchoMinimo, anchoMaximo, t);
            _currentColor = Color.Lerp(_currentColor, colorIdle, Time.deltaTime * velocidadTransicion);
        }

        _currentWidth = Mathf.Lerp(_currentWidth, _targetWidth, Time.deltaTime * velocidadTransicion);

        _renderer.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat(widthProperty, _currentWidth);
        _propBlock.SetColor(colorProperty, _currentColor);
        _renderer.SetPropertyBlock(_propBlock);
    }

    public void SetFocus(bool focus)
    {
        isFocused = focus;
    }

    // --- NUEVO MÉTODO ---
    // Fuerza a que el outline desaparezca inmediatamente cuando el objeto ya no es interactuable
    public void ApagarPorCompleto()
    {
        isFocused = false;

        if (_renderer != null && _propBlock != null)
        {
            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetFloat(widthProperty, 0f); // Ancho a 0 = invisible
            _renderer.SetPropertyBlock(_propBlock);
        }
    }
}