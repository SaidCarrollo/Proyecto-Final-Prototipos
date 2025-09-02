using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using DG.Tweening;

public class GameEditTests
{
    // --- Test 1: GameManager State (Sin cambios) ---
    [Test]
    public void GameManager_HandlePlayerDeath_ChangesStateToLost()
    {
        // ARRANGE: Crear un GameObject y añadir el componente GameManager.
        var go = new GameObject();
        var gameManager = go.AddComponent<GameManager>();

        // ACT: Llamar al método que debería cambiar el estado.
        gameManager.HandlePlayerDeath();

        // ASSERT: Usamos la reflexión para acceder al estado privado y verificarlo.
        var currentStateField = typeof(GameManager).GetField("currentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var currentStateValue = (GameManager.GameState)currentStateField.GetValue(gameManager);

        Assert.AreEqual(GameManager.GameState.Lost, currentStateValue, "El estado del GameManager no cambió a 'Lost' después de la muerte del jugador.");

        // CLEANUP
        Object.DestroyImmediate(go);
    }

    // --- Test 2: Player Movement (Sin cambios) ---
    [Test]
    public void FirstPersonController_StopsHorizontalMovement_WhenInputIsDisabled()
    {
        // ARRANGE: Crear un jugador con Rigidbody y el controlador.
        var playerGO = new GameObject();
        var rb = playerGO.AddComponent<Rigidbody>();
        rb.useGravity = false;
        var controller = playerGO.AddComponent<FirstPersonController>();

        rb.velocity = new Vector3(10f, 0, 10f);

        // ACT: Llamar al método para deshabilitar el input.
        controller.SetInputEnabled(false);

        // ASSERT: Verificar que la velocidad en los ejes X y Z es cero.
        Assert.AreEqual(0f, rb.velocity.x, "La velocidad en X no es cero.");
        Assert.AreEqual(0f, rb.velocity.z, "La velocidad en Z no es cero.");

        // CLEANUP
        Object.DestroyImmediate(playerGO);
    }

    // --- Test 3 (Modificado): Oven Interaction (Sin cambios en su lógica) ---
    [Test]
    public void OvenInteractable_SetsItselfAsUsed_AfterInteraction()
    {
        // ARRANGE
        var ovenGO = new GameObject();
        var oven = ovenGO.AddComponent<OvenInteractable>();
        var gameManagerGO = new GameObject();
        var gameManager = gameManagerGO.AddComponent<GameManager>();

        // Asignar dependencias mínimas para que no falle
        var gameManagerField = typeof(OvenInteractable).GetField("gameManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        gameManagerField.SetValue(oven, gameManager);
        var badgeManagerField = typeof(OvenInteractable).GetField("badgeManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        badgeManagerField.SetValue(oven, ScriptableObject.CreateInstance<BadgeManager>());

        // ACT: Simular la interacción.
        oven.Interact();

        // ASSERT: Usamos reflexión para ver el estado de la variable privada 'hasBeenUsed'.
        var hasBeenUsedField = typeof(OvenInteractable).GetField("hasBeenUsed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsTrue((bool)hasBeenUsedField.GetValue(oven), "El horno debería marcarse como usado tras la interacción.");

        // CLEANUP
        Object.DestroyImmediate(ovenGO);
        Object.DestroyImmediate(gameManagerGO);
    }

    // --- Test 4 (NUEVO): Prueba para el evento de Interactable ---
    [Test]
    public void Interactable_InvokesOnInteract_EventWhenCalled()
    {
        // ARRANGE
        var interactableGO = new GameObject();
        var interactable = interactableGO.AddComponent<Interactable>();
        bool eventWasFired = false;

        // Añadir un "listener" al evento para que cambie nuestro booleano a true.
        interactable.OnInteract.AddListener(() => { eventWasFired = true; });

        // ACT
        // Llamar al método que debería disparar el evento.
        interactable.Interact();

        // ASSERT
        // Verificar que el booleano ahora es true, confirmando que el evento se disparó.
        Assert.IsTrue(eventWasFired, "El evento OnInteract no fue invocado cuando se llamó a Interact().");

        // CLEANUP
        Object.DestroyImmediate(interactableGO);
    }
}