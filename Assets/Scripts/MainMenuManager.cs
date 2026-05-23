using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using System.IO;

public class MainMenuManager : MonoBehaviour
{
    [Header("Elementos de la UI")]
    public TextMeshProUGUI promptText;
    public GameObject buttonContainer;
    public CanvasGroup buttonCanvasGroup;
    public Button continueButton;

    [Header("Panel de Confirmación")]
    public GameObject overwritePanel;

    [Header("Panel de Opciones")]
    public GameObject optionsPanel;
    public Slider sfxSlider;
    public Slider musicSlider;

    [Header("Configuración")]
    public string cinematicSceneName = "CinematicScene";
    public string firstLevelName = "GameScene";

    [Header("Efecto Parpadeo y Fade")]
    public float blinkSpeed = 1.5f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 1.0f;
    public float buttonFadeDuration = 0.5f;

    [Header("Música y Sonido")]
    public AudioSource menuMusic;

    [Range(0f, 1f)]
    public float baseMusicVolume = 0.5f;

    public float musicFadeDuration = 1f;
    public AudioSource sfxSource;
    public AudioClip promptSound;
    public AudioClip buttonClickSound;

    private bool isWaitingForInput = true;
    private string saveLocation;

    void Start()
    {
        buttonContainer.SetActive(false);
        promptText.gameObject.SetActive(true);

        if (overwritePanel != null) overwritePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false); // Apagamos el panel al iniciar

        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");

        bool hasSaveData = File.Exists(saveLocation);

        if (continueButton != null)
        {
            continueButton.interactable = hasSaveData;
        }

        if (menuMusic != null)
        {
            SoundEffectManager.RegisterBackgroundMusic(menuMusic, baseMusicVolume);
        }

        if (sfxSource != null)
        {
            sfxSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        }

        if (SoundEffectManager.Instance != null && sfxSlider != null && musicSlider != null)
        {
            SoundEffectManager.Instance.ConnectSliders(sfxSlider, musicSlider);

            sfxSlider.onValueChanged.AddListener(UpdateLocalSFXVolume);
        }
    }

    void Update()
    {
        if (isWaitingForInput)
        {
            if (promptText != null)
            {
                Color textColor = promptText.color;
                textColor.a = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(Time.time * blinkSpeed, 1f));
                promptText.color = textColor;
            }

            bool inputDetected = false;

            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                inputDetected = true;
            }
            else if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame))
            {
                inputDetected = true;
            }

            if (inputDetected)
            {
                if (sfxSource != null && promptSound != null)
                {
                    sfxSource.PlayOneShot(promptSound);
                }

                ShowMenuButtons();
            }
        }
    }

    private void ShowMenuButtons()
    {
        isWaitingForInput = false;
        promptText.gameObject.SetActive(false);

        StartCoroutine(FadeInButtons());
    }

    private IEnumerator FadeInButtons()
    {
        buttonContainer.SetActive(true);
        buttonCanvasGroup.alpha = 0f;

        buttonCanvasGroup.interactable = false;
        buttonCanvasGroup.blocksRaycasts = false;

        float timer = 0f;
        while (timer < buttonFadeDuration)
        {
            timer += Time.deltaTime;
            buttonCanvasGroup.alpha = timer / buttonFadeDuration;
            yield return null;
        }

        buttonCanvasGroup.alpha = 1f;
        buttonCanvasGroup.interactable = true;
        buttonCanvasGroup.blocksRaycasts = true;
    }

    public void PlayClickSound()
    {
        if (sfxSource != null && buttonClickSound != null)
        {
            sfxSource.PlayOneShot(buttonClickSound);
        }
    }

    public void StartNewGame()
    {
        PlayClickSound();

        if (File.Exists(saveLocation))
        {
            if (overwritePanel != null) overwritePanel.SetActive(true);
        }
        else
        {
            ConfirmNewGame();
        }
    }

    public void ConfirmNewGame()
    {
        PlayClickSound();

        if (overwritePanel != null) overwritePanel.SetActive(false);

        if (File.Exists(saveLocation))
        {
            File.Delete(saveLocation);
            Debug.Log("Partida anterior borrada. Empezando de cero.");
        }

        PlayerPrefs.DeleteKey("TutorialCompleted");
        PlayerPrefs.Save();

        StartCoroutine(FadeMusicAndLoadScene(cinematicSceneName));
    }

    public void CancelNewGame()
    {
        PlayClickSound();
        if (overwritePanel != null) overwritePanel.SetActive(false);
    }

    public void ContinueGame()
    {
        Debug.Log("Continuando partida...");
        PlayClickSound();
        StartCoroutine(FadeMusicAndLoadScene(firstLevelName));
    }

    public void OpenOptions()
    {
        PlayClickSound();
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (buttonContainer != null) buttonContainer.SetActive(false);
    }

    public void CloseOptions()
    {
        PlayClickSound();
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (buttonContainer != null) buttonContainer.SetActive(true);
        PlayerPrefs.Save(); // Guardamos el volumen al cerrar
    }
    public void QuitGame()
    {
        PlayClickSound(); // Hace que suene el clic del botón
        Debug.Log("¡Saliendo del juego!");
        Application.Quit(); // Esta es la orden mágica que cierra la ventana
    }

    private IEnumerator FadeMusicAndLoadScene(string sceneToLoad)
    {
        if (menuMusic != null)
        {
            float startVolume = menuMusic.volume;
            float timer = 0f;

            while (timer < musicFadeDuration)
            {
                timer += Time.deltaTime;
                menuMusic.volume = Mathf.Lerp(startVolume, 0f, timer / musicFadeDuration);
                yield return null;
            }
        }

        SceneManager.LoadScene(sceneToLoad);
    }
    private void UpdateLocalSFXVolume(float newVolume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = newVolume;
        }
    }

}