using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class GameManagerQaTests
{
    private List<Object> testCleanUpObjects; // Puede ser Object para incluir ScriptableObjects

    [SetUp]
    public void SetUp()
    {
        testCleanUpObjects = new List<Object>();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in testCleanUpObjects)
        {
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
            }
        }
        testCleanUpObjects.Clear();
        System.GC.Collect();
    }

    [Test]
    public void GameManager_EndsGame_WhenPrincipalBadgeIsUnlocked()
    {
        // ARRANGE
        var gameManagerGO = new GameObject();
        testCleanUpObjects.Add(gameManagerGO); // <-- AÑADIR A LA LISTA
        var gameManager = gameManagerGO.AddComponent<GameManager>();

        var badgeManager = ScriptableObject.CreateInstance<BadgeManager>();
        testCleanUpObjects.Add(badgeManager); // <-- AÑADIR A LA LISTA

        // Crear badges de prueba
        var principalBadge = new Badge { ID = "MAIN_QUEST", Prioridad = BadgePriority.Principal };
        var badgeList = new List<Badge> { principalBadge };

        // Inyectar dependencias y estado inicial usando reflexión.
        typeof(BadgeManager).GetField("todosLosBadges", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(badgeManager, badgeList);
        typeof(BadgeManager).GetMethod("InicializarManager", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(badgeManager, null);
        typeof(GameManager).GetField("badgeManager", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(gameManager, badgeManager);
        typeof(GameManager).GetField("winSceneName", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(gameManager, "WinScene");

        // Guardar y resetear Time.timeScale.
        var originalTimeScale = Time.timeScale;
        Time.timeScale = 1f;

        // --- ACT ---
        badgeManager.UnlockBadge("MAIN_QUEST");
        gameManager.HandlePlayerSurvival();

        // --- ASSERT ---
        var currentStateField = typeof(GameManager).GetField("currentState", BindingFlags.NonPublic | BindingFlags.Instance);
        var finalState = (GameManager.GameState)currentStateField.GetValue(gameManager);

        Assert.AreEqual(0f, Time.timeScale, "El juego debería pausarse llamando a HandlePlayerSurvival.");
        Assert.AreEqual(GameManager.GameState.Won, finalState, "El estado del juego no cambió a 'Won'.");

        // CLEANUP
        Time.timeScale = originalTimeScale; // Restaurar Time.timeScale es importante.
    }
}