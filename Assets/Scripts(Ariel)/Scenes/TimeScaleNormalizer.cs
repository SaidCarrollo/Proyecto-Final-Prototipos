// TimeScaleNormalizer.cs
using UnityEngine;

public class TimeScaleNormalizer : MonoBehaviour
{
    [SerializeField] private bool forceOnEnable = true;

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        if (forceOnEnable) Time.timeScale = 1f;
    }
}
