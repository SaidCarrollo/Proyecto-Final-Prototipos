using UnityEngine;

public class LaserPointer : MonoBehaviour
{
    [Header("Laser Settings")]
    [SerializeField] private LineRenderer laserLine;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float maxLaserLength = 10f;
    [SerializeField] private Color laserColor = Color.red;

    private GameObject heldObject;

    void Start()
    {
        // Configura el Line Renderer
        laserLine.startWidth = 0.02f;
        laserLine.endWidth = 0.02f;
        laserLine.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        laserLine.material.color = laserColor;
        laserLine.enabled = false;
    }

    void Update()
    {
        if (heldObject != null)
        {
            // Dibuja el l�ser desde la c�mara al objeto
            laserLine.enabled = true;
            laserLine.SetPosition(0, cameraTransform.position);
            laserLine.SetPosition(1, heldObject.transform.position);
        }
        else
        {
            // Desactiva el l�ser si no hay objeto agarrado
            laserLine.enabled = false;
        }
    }

    // Llama a estos m�todos desde tu ObjectGrabber.cs
    public void OnObjectGrabbed(GameObject obj)
    {
        heldObject = obj;
    }

    public void OnObjectReleased()
    {
        heldObject = null;
    }
}