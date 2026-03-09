using UnityEngine;

public abstract class AssistedModeBase : MonoBehaviour
{
    // Cualquier script que herede de este, ESTÁ OBLIGADO a tener esta función
    public abstract void ActivarModoAsistido();

    // Opcional: Si necesitas una lógica de desactivado genérica
    public virtual void DesactivarModoAsistido()
    {
        this.enabled = false;
    }
}