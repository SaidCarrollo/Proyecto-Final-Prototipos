using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using DG.Tweening;

public class GameEditTests
{
    // --- Test 1  GameManager State ---
    // Se convierte a [UnityTest] porque HandlePlayerDeath inicia una Coroutine.
    // Aunque el cambio de estado es inmediato, es una buena práctica probar
    // los métodos que interactúan con el motor de juego en Play Mode.
    [UnityTest]
    public IEnumerator GameManager_HandlePlayerDeath_ChangesStateToLost()
    {
        // ARRANGE: Crear un GameObject y añadir el componente GameManager.
        var go = new GameObject();
        var gameManager = go.AddComponent<GameManager>();

        // ACT: Llamar al método que debería cambiar el estado.
        gameManager.HandlePlayerDeath();

        // Dejamos pasar un frame para asegurar que cualquier lógica inicial se procese.
        yield return null;

        // ASSERT: Usamos la reflexión para acceder al estado privado y verificarlo.
        var currentStateField = typeof(GameManager).GetField("currentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var currentStateValue = (GameManager.GameState)currentStateField.GetValue(gameManager);

        Assert.AreEqual(GameManager.GameState.Lost, currentStateValue, "El estado del GameManager no cambió a 'Lost' después de la muerte del jugador.");

        // CLEANUP
        Object.Destroy(go); // Usar Object.Destroy en Play Mode
    }
    // --- Test 2: Player Movement (Sin cambios) ---
    // --- Test 2 (CORREGIDO): Player Movement ---
    // Se convierte a [UnityTest] porque estamos probando el comportamiento de un Rigidbody,
    // que es parte del motor de físicas y solo se actualiza en Play Mode.
    [UnityTest]
    public IEnumerator FirstPersonController_StopsHorizontalMovement_WhenInputIsDisabled()
    {
        // ARRANGE: Crear un jugador con Rigidbody y el controlador.
        var playerGO = new GameObject();
        var rb = playerGO.AddComponent<Rigidbody>();
        rb.useGravity = false; // Desactivar gravedad para que no interfiera en el test
        var controller = playerGO.AddComponent<FirstPersonController>();

        // Asignar dependencias mínimas (aunque en este caso no son críticas para el método)
        var cameraGO = new GameObject("Camera");
        cameraGO.transform.SetParent(playerGO.transform);
        var cameraTransformField = typeof(FirstPersonController).GetField("cameraTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        cameraTransformField.SetValue(controller, cameraGO.transform);

        rb.linearVelocity = new Vector3(10f, 0, 10f);

        // ACT: Llamar al método para deshabilitar el input.
        controller.SetInputEnabled(false);

        // ESPERAR: Debemos esperar a que el motor de físicas procese un ciclo.
        yield return new WaitForFixedUpdate();

        // ASSERT: Verificar que la velocidad en los ejes X y Z es cero.
        // Usamos Mathf.Approximately para comparar floats de forma segura.
        Assert.IsTrue(Mathf.Approximately(0f, rb.linearVelocity.x), $"La velocidad en X no es cero. Valor: {rb.linearVelocity.x}");
        Assert.IsTrue(Mathf.Approximately(0f, rb.linearVelocity.z), $"La velocidad en Z no es cero. Valor: {rb.linearVelocity.z}");

        // CLEANUP
        Object.Destroy(playerGO);
    }
    // --- Test 3 (Modificado): Oven Interaction (Sin cambios en su l�gica) ---
    // --- Test 3 (CORREGIDO): Oven Interaction ---
    // Convertido a Play Mode para asegurar que las dependencias (GameManager, etc.)
    // se comporten como en el juego real y evitar NullReferenceException.
    [UnityTest]
    public IEnumerator OvenInteractable_SetsItselfAsUsed_AfterInteraction()
    {
        // ARRANGE
        var ovenGO = new GameObject();
        var oven = ovenGO.AddComponent<OvenInteractable>();
        var gameManagerGO = new GameObject();
        var gameManager = gameManagerGO.AddComponent<GameManager>();

        // Asignar dependencias
        var gameManagerField = typeof(OvenInteractable).GetField("gameManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        gameManagerField.SetValue(oven, gameManager);

        var badgeManagerField = typeof(OvenInteractable).GetField("badgeManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        badgeManagerField.SetValue(oven, ScriptableObject.CreateInstance<BadgeManager>()); // Creamos una instancia para que no sea null

        var vignetteEventField = typeof(OvenInteractable).GetField("vignetteEvent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        vignetteEventField.SetValue(oven, ScriptableObject.CreateInstance<VignetteEvent>()); // Asumiento que VignetteEvent es un ScriptableObject

        // ACT: Simular la interacción.
        oven.Interact();

        yield return null;

        // ASSERT: Usamos reflexión para ver el estado de la variable privada 'hasBeenUsed'.
        var hasBeenUsedField = typeof(OvenInteractable).GetField("hasBeenUsed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsTrue((bool)hasBeenUsedField.GetValue(oven), "El horno debería marcarse como usado tras la interacción.");

        // CLEANUP
        Object.Destroy(ovenGO);
        Object.Destroy(gameManagerGO);
    }

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