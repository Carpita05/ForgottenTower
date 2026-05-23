using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using System.IO;

// Este script controla toda la Pantalla de Inicio del juego.
// Gestiona el texto parpadeante de "Pulsa un botón", comprueba si tienes 
// partidas guardadas para dejarte continuar, y maneja las opciones de volumen.
public class MainMenuManager : MonoBehaviour
{
    [Header("Elementos de la Interfaz")]
    public TextMeshProUGUI promptText;       // El texto de "Pulsa cualquier botón"
    public GameObject buttonContainer;       // La caja que agrupa todos los botones (Jugar, Opciones, etc.)
    public CanvasGroup buttonCanvasGroup;    // Para hacer que los botones aparezcan suavemente
    public Button continueButton;            // El botón de Continuar partida

    [Header("Paneles Extra")]
    public GameObject overwritePanel;        // El cartel de "Aviso: Vas a borrar tu partida. ¿Seguro?"
    public GameObject optionsPanel;          // La pantalla de ajustes
    public Slider sfxSlider;                 // Barra de volumen de efectos
    public Slider musicSlider;               // Barra de volumen de música

    [Header("Configuración de Niveles")]
    public string cinematicSceneName = "CinematicScene"; // El vídeo de la intro
    public string firstLevelName = "GameScene";          // El nivel jugable

    [Header("Efectos Visuales")]
    public float blinkSpeed = 1.5f;          // Velocidad de parpadeo del texto
    public float minAlpha = 0.2f;            // Transparencia mínima del texto
    public float maxAlpha = 1.0f;            // Transparencia máxima del texto
    public float buttonFadeDuration = 0.5f;  // Cuánto tardan en aparecer los botones

    [Header("Música y Sonido")]
    public AudioSource menuMusic;
    [Range(0f, 1f)] public float baseMusicVolume = 0.5f;
    public float musicFadeDuration = 1f;     // Cuánto tarda en apagarse la música al darle a Jugar

    public AudioSource sfxSource;
    public AudioClip promptSound;            // Sonido al pulsar el primer botón
    public AudioClip buttonClickSound;       // Sonido de los menús

    private bool isWaitingForInput = true;   // ¿Estamos en la pantalla inicial de "Pulsa un botón"?
    private string saveLocation;             // Ruta del ordenador donde se guarda la partida

    void Start()
    {
        // Al arrancar, escondemos los botones y mostramos solo el texto inicial
        buttonContainer.SetActive(false);
        promptText.gameObject.SetActive(true);

        if (overwritePanel != null) overwritePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        // Buscamos dónde está el archivo de guardado en el disco duro
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        bool hasSaveData = File.Exists(saveLocation);

        // Si no hay partida guardada, bloqueamos el botón de "Continuar" para que no se pueda pulsar
        if (continueButton != null)
        {
            continueButton.interactable = hasSaveData;
        }

        // Configuramos la música y los volúmenes leyendo las preferencias guardadas del jugador
        if (menuMusic != null) SoundEffectManager.RegisterBackgroundMusic(menuMusic, baseMusicVolume);
        if (sfxSource != null) sfxSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (SoundEffectManager.Instance != null && sfxSlider != null && musicSlider != null)
        {
            SoundEffectManager.Instance.ConnectSliders(sfxSlider, musicSlider);
            sfxSlider.onValueChanged.AddListener(UpdateLocalSFXVolume);
        }
    }

    void Update()
    {
        // Si estamos en la pantalla inicial esperando a que el jugador pulse algo...
        if (isWaitingForInput)
        {
            // 1. Hacemos que el texto parpadee suavemente de forma matemática (PingPong)
            if (promptText != null)
            {
                Color textColor = promptText.color;
                textColor.a = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(Time.time * blinkSpeed, 1f));
                promptText.color = textColor;
            }

            // 2. Comprobamos si el jugador ha tocado el teclado o el ratón
            bool inputDetected = false;
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) inputDetected = true;
            else if (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)) inputDetected = true;

            // 3. Si ha tocado algo, pasamos al Menú de verdad
            if (inputDetected)
            {
                if (sfxSource != null && promptSound != null) sfxSource.PlayOneShot(promptSound);
                ShowMenuButtons();
            }
        }
    }

    // Oculta el texto parpadeante y arranca la aparición de los botones
    private void ShowMenuButtons()
    {
        isWaitingForInput = false;
        promptText.gameObject.SetActive(false);
        StartCoroutine(FadeInButtons());
    }

    // Hace aparecer los botones suavemente (Fundido / Fade In)
    private IEnumerator FadeInButtons()
    {
        buttonContainer.SetActive(true);
        buttonCanvasGroup.alpha = 0f;

        // Bloqueamos clics accidentales mientras aparecen
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
        if (sfxSource != null && buttonClickSound != null) sfxSource.PlayOneShot(buttonClickSound);
    }

    // --- ACCIONES DE LOS BOTONES ---

    public void StartNewGame()
    {
        PlayClickSound();
        // Si ya hay una partida guardada, mostramos el aviso de sobrescribir antes de borrar nada
        if (File.Exists(saveLocation))
        {
            if (overwritePanel != null) overwritePanel.SetActive(true);
        }
        else
        {
            ConfirmNewGame();
        }
    }

    // Borra todo el progreso anterior y empieza el vídeo de la historia
    public void ConfirmNewGame()
    {
        PlayClickSound();
        if (overwritePanel != null) overwritePanel.SetActive(false);

        // Destruimos el archivo de guardado físico del PC
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

    // --- OPCIONES Y SALIDA ---

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
        PlayerPrefs.Save(); // Guardamos el volumen en disco al cerrar
    }

    public void QuitGame()
    {
        PlayClickSound();
        Debug.Log("¡Saliendo del juego!");
        Application.Quit(); // Cierra el juego en la versión compilada (.exe)
    }

    // Apaga la música progresivamente antes de cargar el siguiente nivel para que no suene un corte brusco
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
        if (sfxSource != null) sfxSource.volume = newVolume;
    }
}