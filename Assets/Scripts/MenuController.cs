using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // --- NUEVO: Necesario para poder detectar los Sliders ---

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;

    [Header("Sliders de Sonido (Pausa)")]
    public Slider sfxSlider;
    public Slider musicSlider;

    void Start()
    {
        menuCanvas.SetActive(false);

        if (SoundEffectManager.Instance != null && sfxSlider != null && musicSlider != null)
        {
            SoundEffectManager.Instance.ConnectSliders(sfxSlider, musicSlider);
        }
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (FadeTrigger.isTransitioning) return;

            if (!menuCanvas.activeSelf && PauseController.IsGamePaused)
            {
                return;
            }
            menuCanvas.SetActive(!menuCanvas.activeSelf);
            PauseController.SetPause(menuCanvas.activeSelf);

            if (menuCanvas.activeSelf && SoundEffectManager.Instance != null && sfxSlider != null && musicSlider != null)
            {
                SoundEffectManager.Instance.ConnectSliders(sfxSlider, musicSlider);
            }
        }
    }
}