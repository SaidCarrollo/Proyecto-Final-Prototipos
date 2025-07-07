
using UnityEngine;
using UnityEngine.UI; 

public class ButtonSound : MonoBehaviour
{
    private Button button;
    [SerializeField] AudioClip Click;

    private void Start()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        // Antes: AudioManager.instance.ToggleMute();
        // Ahora: AudioManager.Instance.ToggleMute(); // <-- Se usa 'Instance' con may�scula
        AudioManager.Instance.PlaySFX(Click);
    }
}