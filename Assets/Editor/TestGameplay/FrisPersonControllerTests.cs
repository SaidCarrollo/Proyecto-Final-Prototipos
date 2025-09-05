using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Reflection;

public class FirstPersonController_Test
{
    private GameObject playerGO;
    private Rigidbody rb;
    private FirstPersonController controller;
    private Transform cameraTransform;

    // El [SetUp] se ejecuta ANTES de cada test.
    // Prepara la escena para que cada prueba empiece desde cero.
    [SetUp]
    public void Setup()
    {
        // ARRANGE
        playerGO = new GameObject("Player");
        rb = playerGO.AddComponent<Rigidbody>();
        rb.useGravity = false; // Desactivar gravedad para tests predecibles.
        rb.linearDamping = 0; // Desactivar fricci�n.

        var cameraGO = new GameObject("Camera");
        cameraTransform = cameraGO.transform;
        cameraTransform.SetParent(playerGO.transform);

        controller = playerGO.AddComponent<FirstPersonController>();

        // Usar reflexi�n para inyectar la referencia a la c�mara.
        typeof(FirstPersonController)
            .GetField("cameraTransform", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(controller, cameraTransform);
    }

    // El [TearDown] se ejecuta DESPU�S de cada test.
    // Limpia la escena para no dejar objetos basura.
    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(playerGO);
    }

    // --- TEST 1: Movimiento Hacia Adelante ---
    [UnityTest]
    public IEnumerator MovesForward_WhenGivenForwardInput()
    {
        // ACT
        // 1. Simular el input del jugador (presionando 'W').
        var movementInputField = typeof(FirstPersonController).GetField("currentMovementInput", BindingFlags.NonPublic | BindingFlags.Instance);
        movementInputField.SetValue(controller, new Vector2(0, 1));

        // 2. Esperar un ciclo de f�sicas para que el componente ejecute su FixedUpdate().
        yield return new WaitForFixedUpdate();

        // ASSERT
        // 3. Verificar que la velocidad en el eje Z sea la correcta (walkSpeed).
        float walkSpeed = (float)typeof(FirstPersonController).GetField("walkSpeed", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(controller);
        Assert.AreEqual(walkSpeed, rb.linearVelocity.z, 0.1f, "El jugador no se movi� hacia adelante a la velocidad de caminata.");
    }

    // --- TEST 2: Salto ---
    [UnityTest]
    public IEnumerator Jumps_WhenGroundedAndJumpIsTriggered()
    {
        // ARRANGE ADICIONAL
        // 1. Simular que el jugador est� en el suelo.
        var isGroundedField = typeof(FirstPersonController).GetField("isGrounded", BindingFlags.NonPublic | BindingFlags.Instance);
        isGroundedField.SetValue(controller, true);

        // 2. Obtener el m�todo privado de salto.
        var handleJumpMethod = typeof(FirstPersonController).GetMethod("HandleJump", BindingFlags.NonPublic | BindingFlags.Instance);

        // ACT
        // 3. Invocar el salto.
        handleJumpMethod.Invoke(controller, null);

        // 4. Esperar un ciclo de f�sicas.
        yield return new WaitForFixedUpdate();

        // ASSERT
        // 5. Verificar que la velocidad vertical sea igual a la fuerza de salto.
        float jumpForce = (float)typeof(FirstPersonController).GetField("jumpForce", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(controller);
        Assert.AreEqual(jumpForce, rb.linearVelocity.y, 0.1f, "El jugador no salt� con la fuerza esperada.");
    }

    // --- TEST 3: Rotaci�n de la C�mara ---
    [UnityTest]
    public IEnumerator RotatesCorrectly_WhenGivenLookInput()
    {
        // ARRANGE
        var initialPlayerRotation = playerGO.transform.rotation;
        var initialCameraRotation = cameraTransform.localRotation;
        var handleLookMethod = typeof(FirstPersonController).GetMethod("HandleLook", BindingFlags.NonPublic | BindingFlags.Instance);

        // ACT
        // 1. Simular un movimiento grande y claro del mouse hacia la derecha y hacia arriba.
        handleLookMethod.Invoke(controller, new object[] { new Vector2(500, -500) });
        yield return null; // Esperar un frame normal para que se aplique la rotaci�n.

        // ASSERT
        // 2. El cuerpo del jugador (eje Y) debe haber rotado.
        Assert.AreNotEqual(initialPlayerRotation.eulerAngles.y, playerGO.transform.rotation.eulerAngles.y, "El cuerpo del jugador no rot� horizontalmente.");

        // 3. La c�mara (eje X) debe haber rotado.
        Assert.AreNotEqual(initialCameraRotation.eulerAngles.x, cameraTransform.localRotation.eulerAngles.x, "La c�mara no rot� verticalmente.");
    }
}