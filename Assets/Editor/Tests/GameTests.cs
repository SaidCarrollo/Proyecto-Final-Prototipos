using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GameTests
{
    [Test]
    public void Interactable_EventFires_WhenInteractIsCalled()
    {
        // ARRANGE: Crear el objeto y el componente a probar.
        var interactableGO = new GameObject();
        var interactable = interactableGO.AddComponent<Interactable>();
        bool eventWasFired = false;

        // Suscribir una acción muy simple al evento.
        // Esto hace que nuestra variable 'eventWasFired' se vuelva 'true' cuando el evento se dispare.
        interactable.OnInteract.AddListener(() => { eventWasFired = true; });

        // ACT: Llamar al método que queremos probar.
        interactable.Interact();

        // ASSERT: Comprobar el resultado.
        // La variable 'eventWasFired' DEBERÍA ser 'true' si el evento funcionó.
        Assert.IsTrue(eventWasFired, "El evento OnInteract no fue disparado cuando se llamó a Interact().");

        // CLEANUP: Destruir el objeto de prueba.
        Object.DestroyImmediate(interactableGO);
    }
}
