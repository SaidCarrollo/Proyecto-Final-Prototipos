
using UnityEngine;

public enum BadgeType
{
    Correcto,
    Incorrecto
}

[System.Serializable]
public class Badge
{
    public string ID;

    [Tooltip("El texto que se mostrará en la UI final.")]
    [TextArea(2, 5)]
    public string Descripcion;

    [Tooltip("Define si es un logro (Correcto) o un error (Incorrecto).")]
    public BadgeType Tipo;

    [HideInInspector]
    public bool Desbloqueado = false;
}