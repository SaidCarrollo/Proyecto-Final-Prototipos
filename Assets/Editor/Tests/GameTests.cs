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

        // Suscribir una acci�n muy simple al evento.
        // Esto hace que nuestra variable 'eventWasFired' se vuelva 'true' cuando el evento se dispare.
        interactable.OnInteract.AddListener(() => { eventWasFired = true; });

        // ACT: Llamar al m�todo que queremos probar.
        interactable.Interact();

        // ASSERT: Comprobar el resultado.
        // La variable 'eventWasFired' DEBER�A ser 'true' si el evento funcion�.
        Assert.IsTrue(eventWasFired, "El evento OnInteract no fue disparado cuando se llam� a Interact().");

        // CLEANUP: Destruir el objeto de prueba.
        Object.DestroyImmediate(interactableGO);
    }
}
