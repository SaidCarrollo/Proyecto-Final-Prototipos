using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class MissingQaTests
{
    // --- Test para QA-01 y QA-02: Movimiento y Cámara del Personaje (Versión Robusta) ---
    [UnityTest]
    public IEnumerator FirstPersonController_MovementAndLook_RespondsCorrectly()
    {
        // ARRANGE
        var playerGO = new GameObject("Player");
        var rb = playerGO.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 0;

        var cameraGO = new GameObject("Camera");
        cameraGO.transform.SetParent(playerGO.transform);

        var controller = playerGO.AddComponent<FirstPersonController>();

        // Usar reflexión para asignar TODAS las dependencias y valores de prueba
        typeof(FirstPersonController).GetField("cameraTransform", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(controller, cameraGO.transform);
        typeof(FirstPersonController).GetField("walkSpeed", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(controller, 5f);
        typeof(FirstPersonController).GetField("mouseSensitivity", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(controller, 100f);

        // Acceder a los métodos privados y campos que procesan el input
        var handleMovementMethod = typeof(FirstPersonController).GetMethod("HandleMovement", BindingFlags.NonPublic | BindingFlags.Instance);
        var handleLookMethod = typeof(FirstPersonController).GetMethod("HandleLook", BindingFlags.NonPublic | BindingFlags.Instance);
        var currentMovementField = typeof(FirstPersonController).GetField("currentMovementInput", BindingFlags.NonPublic | BindingFlags.Instance);

        // --- ACT 1: Simular movimiento hacia adelante ---
        currentMovementField.SetValue(controller, new Vector2(0, 1)); // Simula la tecla 'W'
        handleMovementMethod.Invoke(controller, null);

        yield return new WaitForFixedUpdate(); // Esperar a que la física se aplique

        // --- ASSERT 1: Verificar que el Rigidbody se mueve a la velocidad correcta ---
        // Se calcula la velocidad solo en el plano horizontal (XZ) para ignorar la gravedad/salto
        var horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float expectedSpeed = 5f;

        Assert.AreEqual(expectedSpeed, horizontalVelocity.magnitude, 0.01f,
            $"La velocidad horizontal ({horizontalVelocity.magnitude}) no coincide con la esperada ({expectedSpeed}).");

        // --- ACT 2: Simular rotación de la cámara (mirar a la derecha) ---
        var initialYRotation = playerGO.transform.rotation.eulerAngles.y;
        var lookInput = new Vector2(50, 0); // Un input horizontal suficientemente grande

        handleLookMethod.Invoke(controller, new object[] { lookInput });
        yield return null; // Esperar un frame para que la rotación se aplique

        // --- ASSERT 2: Verificar que el cuerpo del jugador rotó en el eje Y ---
        Assert.AreNotEqual(initialYRotation, playerGO.transform.rotation.eulerAngles.y,
            "La rotación del jugador no cambió después de simular el movimiento del mouse.");

        // CLEANUP
        Object.Destroy(playerGO);
    }
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