using System.Collections;
using NUnit.Framework;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.TestTools;

public class MemoryPerformanceTests
{
    private ProfilerRecorder systemMemoryRecorder;

    [SetUp]
    public void OnEnable()
    {
        // Configurar el recorder para rastrear la memoria total usada por el sistema.
        systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
    }

    [TearDown]
    public void OnDisable()
    {
        // Liberar el recorder para evitar fugas de memoria en el editor.
        systemMemoryRecorder.Dispose();
    }

    [UnityTest]
    public IEnumerator MemoryUsage_StaysWithinBudgetDuringGameplay()
    {
        // ARRANGE
        // L�mite de memoria objetivo en Megabytes (MB).
        const long memoryThresholdMB = 1500; // 1.5 GB para PC
        long maxMemoryUsed = 0;

        // ACT
        // Simular un "gameplay" de 10 segundos.
        // En un proyecto real, aqu� podr�as cargar una escena y ejecutar acciones espec�ficas.
        Debug.Log("Iniciando prueba de monitoreo de memoria por 10 segundos...");
        for (float timer = 0; timer < 10.0f; timer += Time.deltaTime)
        {
            // Tomar la muestra de memoria m�s reciente.
            // El recorder se actualiza cada frame.
            if (systemMemoryRecorder.LastValue > maxMemoryUsed)
            {
                maxMemoryUsed = systemMemoryRecorder.LastValue;
            }
            yield return null;
        }

        // Convertir el resultado de Bytes a Megabytes para que sea m�s legible.
        float maxMemoryUsedMB = maxMemoryUsed / (1024f * 1024f);

        // ASSERT
        Debug.Log($"Pico de memoria detectado: {maxMemoryUsedMB:F2} MB");

        // Esta aserci�n puede fallar a menudo. Su prop�sito principal es notificar
        // si el uso de memoria excede un umbral predefinido.
        Assert.LessOrEqual(maxMemoryUsedMB, memoryThresholdMB,
            $"El pico de memoria ({maxMemoryUsedMB:F2} MB) excedi� el presupuesto de {memoryThresholdMB} MB.");
    }
}