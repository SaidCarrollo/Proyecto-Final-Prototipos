using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PerformanceBudgetTests
{
    [UnityTest]
    public IEnumerator AverageDeltaTime_StaysWithin60FPSBudget()
    {
        // ARRANGE
        const int frameCount = 120; // ~2 segundos a 60 FPS
        float targetDeltaTime = 1f / 60f;
        float deltaTimeSum = 0f;

        // ACT: Esperar un número de frames y sumar el tiempo transcurrido en cada uno
        for (int i = 0; i < frameCount; i++)
        {
            yield return new WaitForEndOfFrame();
            deltaTimeSum += Time.deltaTime;
        }

        // ASSERT: Calcular el promedio y compararlo con el objetivo
        float averageDeltaTime = deltaTimeSum / frameCount;
        Assert.LessOrEqual(averageDeltaTime, targetDeltaTime,
            $"El promedio de frame ({averageDeltaTime:0.000}s) excedió el presupuesto de 60 FPS ({targetDeltaTime:0.000}s).");
    }
}