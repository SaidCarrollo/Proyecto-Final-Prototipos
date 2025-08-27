
using UnityEngine;

public class PlayerZoneState : MonoBehaviour
{
    private int zoneCounter = 0;
    // Devuelve 'true' si el jugador est� dentro de al menos una zona restringida.
    public bool IsInRestrictedZone => zoneCounter > 0;

    public void EnterZone()
    {
        zoneCounter++;
    }

    public void ExitZone()
    {
        if (zoneCounter > 0)
        {
            zoneCounter--;
        }
    }
}