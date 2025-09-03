using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

public class GameEditTests
{
    [Test]
    public void ObjectGrabber_IsHoldingObject_ReturnsCorrectState()
    {
        // ARRANGE
        var grabberGO = new GameObject();
        var grabber = grabberGO.AddComponent<ObjectGrabber>();
        var heldObjectField = typeof(ObjectGrabber).GetField("heldObject", BindingFlags.NonPublic | BindingFlags.Instance);
        var dummyObject = new GameObject();

        // ASSERT 1: Verificar el estado inicial (no debería estar sosteniendo nada).
        Assert.IsFalse(grabber.IsHoldingObject(), "El grabber no debería sostener un objeto al inicio.");

        // ACT 1: Simular que se agarra un objeto usando reflexión.
        heldObjectField.SetValue(grabber, dummyObject);

        // ASSERT 2: Verificar que ahora sí reporta que está sosteniendo un objeto.
        Assert.IsTrue(grabber.IsHoldingObject(), "El grabber debería reportar que sostiene un objeto.");

        // ACT 2: Simular que se suelta el objeto.
        heldObjectField.SetValue(grabber, null);

        // ASSERT 3: Verificar que el estado vuelve a ser 'false'.
        Assert.IsFalse(grabber.IsHoldingObject(), "El grabber debería reportar que ya no sostiene el objeto.");

        // CLEANUP
        Object.DestroyImmediate(grabberGO);
        Object.DestroyImmediate(dummyObject);
    }

    // --- Test 2 (Corregido y Mejorado): Oven Interaction ---
    [UnityTest]
    public IEnumerator OvenInteractable_InteractionUnlocksCorrectBadge()
    {
        // ARRANGE
        var ovenGO = new GameObject();
        var oven = ovenGO.AddComponent<OvenInteractable>();
        var gameManagerGO = new GameObject();
        var gameManager = gameManagerGO.AddComponent<GameManager>();
        var badgeManager = ScriptableObject.CreateInstance<BadgeManager>();

        // Crear una lista de badges para el manager y asignarla vía reflexión
        var testBadge = new Badge { ID = "PrevencionHornilla", Desbloqueado = false };
        var badgeList = new List<Badge> { testBadge };
        var todosLosBadgesField = typeof(BadgeManager).GetField("todosLosBadges", BindingFlags.NonPublic | BindingFlags.Instance);
        todosLosBadgesField.SetValue(badgeManager, badgeList);

        // Inicializar el BadgeManager para que procese la lista
        var inicializarMethod = typeof(BadgeManager).GetMethod("InicializarManager", BindingFlags.NonPublic | BindingFlags.Instance);
        inicializarMethod.Invoke(badgeManager, null);

        // Asignar dependencias al horno (OvenInteractable)
        typeof(OvenInteractable).GetField("gameManager", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(oven, gameManager);
        typeof(OvenInteractable).GetField("badgeManager", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(oven, badgeManager);
        typeof(OvenInteractable).GetField("vignetteEvent", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(oven, ScriptableObject.CreateInstance<VignetteEvent>());

        // ACT: Simular la primera interacción.
        oven.Interact();
        yield return null;

        // ASSERT 1: Verificar que el horno se marca como usado.
        var hasBeenUsedField = typeof(OvenInteractable).GetField("hasBeenUsed", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsTrue((bool)hasBeenUsedField.GetValue(oven), "El horno debería marcarse como usado.");

        // ASSERT 2: Verificar que el badge correcto fue desbloqueado.
        var unlockedBadges = badgeManager.GetUnlockedBadges();
        Assert.AreEqual(1, unlockedBadges.Count, "No se desbloqueó el número esperado de badges.");
        Assert.AreEqual("PrevencionHornilla", unlockedBadges[0].ID, "Se desbloqueó un badge incorrecto.");

        // ACT 2: Interactuar de nuevo no debería hacer nada.
        oven.Interact();
        yield return null;

        // ASSERT 3: La lista de badges desbloqueados no debe cambiar.
        Assert.AreEqual(1, badgeManager.GetUnlockedBadges().Count, "Interactuar de nuevo no debería desbloquear más badges.");

        // CLEANUP
        Object.Destroy(ovenGO);
        Object.Destroy(gameManagerGO);
        Object.Destroy(badgeManager);
    }

    [Test]
    public void ParticleIntensityController_SetIntensity_AdjustsEmissionRate()
    {
        // ARRANGE
        var particleGO = new GameObject();
        var particleSystem = particleGO.AddComponent<ParticleSystem>();
        var controller = particleGO.AddComponent<ParticleIntensityController>();

        // Obtenemos acceso al módulo de emisión para poder leer sus valores
        var emissionModule = particleSystem.emission;

        // Definimos valores de prueba para que el cálculo sea predecible
        float minRate = 10f;
        float maxRate = 110f;
        controller.minEmissionRate = minRate;
        controller.maxEmissionRate = maxRate;

        // ACT 1: Establecer la intensidad al 50%
        controller.SetIntensity(0.5f);

        // ASSERT 1: La tasa de emisión debe ser el punto medio entre min y max.
        // Mathf.Lerp(10, 110, 0.5) = 60
        Assert.AreEqual(60f, emissionModule.rateOverTime.constant, "La tasa de emisión al 50% de intensidad es incorrecta.");

        // ACT 2: Establecer la intensidad al 100%
        controller.SetIntensity(1f);

        // ASSERT 2: La tasa de emisión debe ser igual a la tasa máxima.
        Assert.AreEqual(maxRate, emissionModule.rateOverTime.constant, "La tasa de emisión al 100% no es la máxima.");

        // ACT 3: Establecer la intensidad a 0
        controller.SetIntensity(0f);

        // ASSERT 3: El módulo de emisión debe estar desactivado.
        Assert.IsFalse(emissionModule.enabled, "El módulo de emisión debería estar desactivado con intensidad 0.");

        // CLEANUP
        Object.DestroyImmediate(particleGO);
    }


    // --- Test 4 (Nuevo): BadgeManager Logic ---
    [Test]
    public void BadgeManager_UnlocksAndRetrievesBadges()
    {
        // ARRANGE
        var badgeManager = ScriptableObject.CreateInstance<BadgeManager>();
        var badges = new List<Badge>
        {
            new Badge { ID = "BADGE_01", Tipo = BadgeType.Correcto },
            new Badge { ID = "BADGE_02", Tipo = BadgeType.Incorrecto }
        };
        typeof(BadgeManager).GetField("todosLosBadges", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(badgeManager, badges);
        typeof(BadgeManager).GetMethod("InicializarManager", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(badgeManager, null);

        // ACT: Desbloquear un badge existente y uno no existente.
        badgeManager.UnlockBadge("BADGE_01");
        badgeManager.UnlockBadge("NON_EXISTENT_BADGE");

        var unlockedCorrectos = badgeManager.GetUnlockedBadges(tipo: BadgeType.Correcto);
        var unlockedIncorrectos = badgeManager.GetUnlockedBadges(tipo: BadgeType.Incorrecto);

        // ASSERT 1: Verificar que solo el badge correcto fue desbloqueado.
        Assert.AreEqual(1, unlockedCorrectos.Count, "Debería haber 1 badge correcto desbloqueado.");
        Assert.AreEqual("BADGE_01", unlockedCorrectos[0].ID);

        // ASSERT 2: Verificar que no hay badges incorrectos desbloqueados.
        Assert.AreEqual(0, unlockedIncorrectos.Count, "No deberían haber badges incorrectos desbloqueados.");

        // ASSERT 3: Verificar que el badge está marcado como desbloqueado internamente.
        // (Accedemos al diccionario privado para una prueba más profunda)
        var dictField = typeof(BadgeManager).GetField("badgesDict", BindingFlags.NonPublic | BindingFlags.Instance);
        var dict = (Dictionary<string, Badge>)dictField.GetValue(badgeManager);
        Assert.IsTrue(dict["BADGE_01"].Desbloqueado, "El estado 'Desbloqueado' del badge no se actualizó.");

        // CLEANUP
        Object.DestroyImmediate(badgeManager);
    }

    // --- Test 5 (Nuevo): GrabbableObject State ---
    [UnityTest]
    public IEnumerator GrabbableObject_SetWet_ChangesColorAndState()
    {
        // ARRANGE
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var grabbable = cube.AddComponent<GrabbableObject>();
        var renderer = cube.GetComponent<Renderer>();
        var originalColor = renderer.material.color;

        // Usar reflexión para establecer una duración corta para la prueba
        var wetDurationField = typeof(GrabbableObject).GetField("wetDuration", BindingFlags.NonPublic | BindingFlags.Instance);
        wetDurationField.SetValue(grabbable, 0.1f);
        float testDuration = (float)wetDurationField.GetValue(grabbable);

        // ACT
        grabbable.SetWet(true);
        yield return null; // Esperar un frame para que el cambio de color se aplique

        // ASSERT 1: El estado debe ser "mojado".
        Assert.IsTrue(grabbable.EstaMojado, "El objeto no se marcó como mojado (EstaMojado).");

        // ASSERT 2: El color debe haber cambiado.
        Assert.AreNotEqual(originalColor, renderer.material.color, "El color del material no cambió al mojarse.");

        // ESPERAR a que el objeto se seque
        yield return new WaitForSeconds(testDuration + 0.1f);

        // ASSERT 3: El objeto debe estar seco y haber vuelto a su color original.
        Assert.IsFalse(grabbable.EstaMojado, "El objeto no se secó después de la duración establecida.");

        // CLEANUP
        Object.Destroy(cube);
    }
}