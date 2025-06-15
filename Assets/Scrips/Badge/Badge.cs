
using UnityEngine;
public enum BadgePriority
{
    Principal,
    Secundario
}
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
    [Tooltip("Define si este badge finaliza el juego (Principal) o es un logro/error complementario (Secundario).")]
    public BadgePriority Prioridad = BadgePriority.Secundario; 

    [HideInInspector]
    public bool Desbloqueado = false;
}