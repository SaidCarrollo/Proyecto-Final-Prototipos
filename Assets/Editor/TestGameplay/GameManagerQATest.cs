using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class GameManagerQaTests
{

    // --- Test para QA-09: Lógica de Fin de Partida (Corregido) ---
    [Test]
    public void GameManager_EndsGame_WhenPrincipalBadgeIsUnlocked()
    {
        // ARRANGE
        var gameManagerGO = new GameObject();
        var gameManager = gameManagerGO.AddComponent<GameManager>();

        var badgeManager = ScriptableObject.CreateInstance<BadgeManager>();

        // Crear badges de prueba
        var principalBadge = new Badge { ID = "MAIN_QUEST", Prioridad = BadgePriority.Principal };
        var badgeList = new List<Badge> { principalBadge };

        // Inyectar dependencias y estado inicial usando reflexión.
        typeof(BadgeManager).GetField("todosLosBadges", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(badgeManager, badgeList);
        typeof(BadgeManager).GetMethod("InicializarManager", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(badgeManager, null);
        typeof(GameManager).GetField("badgeManager", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(gameManager, badgeManager);

        // Asignar nombres de escena para que el GameManager no lance un error.
        typeof(GameManager).GetField("winSceneName", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(gameManager, "WinScene");

        // Guardar el estado original de Time.timeScale.
        var originalTimeScale = Time.timeScale;
        Time.timeScale = 1f;

        // --- ACT: Desbloquear el badge principal y luego llamar al método que gestiona la victoria ---
        badgeManager.UnlockBadge("MAIN_QUEST");

        // Esto es lo correcto: llamamos al método PÚBLICO que tu script SÍ tiene.
        gameManager.HandlePlayerSurvival();

        // --- ASSERT: El juego debe terminar (Time.timeScale se pone en 0 y el estado cambia).
        var currentStateField = typeof(GameManager).GetField("currentState", BindingFlags.NonPublic | BindingFlags.Instance);
        var finalState = (GameManager.GameState)currentStateField.GetValue(gameManager);

        Assert.AreEqual(0f, Time.timeScale, "El juego debería pausarse llamando a HandlePlayerSurvival.");
        Assert.AreEqual(GameManager.GameState.Won, finalState, "El estado del juego no cambió a 'Won'.");

        // CLEANUP
        Time.timeScale = originalTimeScale; // Restaurar Time.timeScale para no afectar otros tests.
        Object.DestroyImmediate(gameManagerGO);
        Object.DestroyImmediate(badgeManager);
    }
}