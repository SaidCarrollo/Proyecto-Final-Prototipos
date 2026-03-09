using UnityEngine;
using System.Collections.Generic;
using DG.Tweening; // Importamos DOTween

public class AssistedNode3D : Interactable
{
    [Header("Datos del Nodo")]
    public string nombreDelLugar;
    public Transform puntoDeDestino;

    [Header("Conexiones")]
    public List<AssistedNode3D> nodosVecinos;

    [Header("Visuales y Animación")]
    public GameObject modeloVisual;
    public Collider nodoCollider;
    [Tooltip("Duración del efecto de aparecer/desaparecer en segundos.")]
    public float fadeDuration = 0.5f;

    private AssistedControllerLvl2 _controlador;

    // Variables para el fade
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;
    private Color _originalColor;
    private Tween _fadeTween;

    protected override void Awake()
    {
        base.Awake(); // Llamamos al Awake de Interactable (muy importante para que el Highlighter funcione)

        if (modeloVisual != null)
        {
            _renderer = modeloVisual.GetComponent<Renderer>();
            if (_renderer == null) _renderer = modeloVisual.GetComponentInChildren<Renderer>();
        }

        _propBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        if (_renderer != null)
        {
            // Guardamos el color original del material (para saber hasta qué opacidad debe subir)
            _originalColor = _renderer.material.GetColor("_MainColor");
        }

        // Al iniciar la escena, los apagamos de golpe
        ApagarNodo(true);
    }

    public void Configurar(AssistedControllerLvl2 controlador)
    {
        _controlador = controlador;
    }

    public override void Interact()
    {
        if (_controlador != null)
        {
            _controlador.ViajarANodo(this);
        }
    }

    // --- CONTROL DE VISIBILIDAD ANIMADO ---

    public void EncenderNodo()
    {
        if (modeloVisual != null) modeloVisual.SetActive(true);
        if (nodoCollider != null) nodoCollider.enabled = true;

        if (_renderer != null)
        {
            // Matamos cualquier animación anterior por si acaso
            _fadeTween?.Kill();

            // Empezamos desde invisible
            ActualizarAlpha(0f);

            // Usamos DOVirtual de DOTween para animar un número de 0 hasta el Alpha original
            _fadeTween = DOVirtual.Float(0f, _originalColor.a, fadeDuration, (valorAlpha) =>
            {
                ActualizarAlpha(valorAlpha);
            });
        }
    }

    public void ApagarNodo(bool instantaneo = false)
    {
        // Desactivamos el collider de inmediato para que el jugador no pueda clickearlo mientras desaparece
        if (nodoCollider != null) nodoCollider.enabled = false;

        if (instantaneo || _renderer == null || !modeloVisual.activeSelf)
        {
            if (modeloVisual != null) modeloVisual.SetActive(false);
            return;
        }

        _fadeTween?.Kill();

        // Animar desde la opacidad actual hasta 0
        float alphaActual = _originalColor.a;
        _fadeTween = DOVirtual.Float(alphaActual, 0f, fadeDuration, (valorAlpha) =>
        {
            ActualizarAlpha(valorAlpha);
        }).OnComplete(() =>
        {
            // Solo desactivamos el GameObject cuando la animación termina por completo
            if (modeloVisual != null) modeloVisual.SetActive(false);
        });
    }

    // Método interno súper optimizado para cambiar el color del Shader
    private void ActualizarAlpha(float alpha)
    {
        if (_renderer == null) return;

        // 1. Obtenemos el bloque actual (para no borrar lo que hace el ObjectHighlighter)
        _renderer.GetPropertyBlock(_propBlock);

        // 2. Modificamos solo el color base
        Color colorModificado = _originalColor;
        colorModificado.a = alpha;
        _propBlock.SetColor("_MainColor", colorModificado);

        // 3. Aplicamos
        _renderer.SetPropertyBlock(_propBlock);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        if (puntoDeDestino != null) Gizmos.DrawSphere(puntoDeDestino.position, 0.2f);

        if (nodosVecinos != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var vecino in nodosVecinos)
            {
                if (vecino != null) Gizmos.DrawLine(transform.position, vecino.transform.position);
            }
        }
    }
}