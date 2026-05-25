using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

// Sistema de Tutorial por Eventos
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI del Tutorial")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;

    // -1 significa que no hay ningún tutorial mostrándose ahora mismo
    private int activeTutorial = -1;

    private void Awake()
    {
        // Lo convertimos en Singleton para poder llamarlo desde las zonas (Triggers)
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        tutorialPanel.SetActive(false);
        // El primer tutorial (Moverse) intenta saltar nada más iniciar la partida
        ShowTutorial(0);
    }

    void Update()
    {
        // Si no hay panel activo, no leemos teclas para el tutorial
        if (!tutorialPanel.activeSelf || activeTutorial == -1) return;

        bool stepCompleted = false;

        // Comprobamos qué tecla debe pulsar según el tutorial que esté en pantalla
        switch (activeTutorial)
        {
            case 0: // Tutorial 1: Moverse
                if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame ||
                    Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
                    stepCompleted = true;
                break;

            case 1: // Tutorial 2: Interactuar
                if (Keyboard.current.cKey.wasPressedThisFrame) stepCompleted = true;
                break;

            case 2: // Tutorial 3: Atacar
                if (Keyboard.current.zKey.wasPressedThisFrame) stepCompleted = true;
                break;

            case 3: // Tutorial 4: Abrir el Menú
                if (Keyboard.current.tabKey.wasPressedThisFrame) stepCompleted = true;
                break;

            case 4: // Tutorial 5: Hotbar
                bool pressedHotbarKey = false;
                for (int i = (int)Key.Digit1; i <= (int)Key.Digit0; i++)
                {
                    if (Keyboard.current[(Key)i].wasPressedThisFrame)
                    {
                        pressedHotbarKey = true;
                        break;
                    }
                }
                if (Keyboard.current.cKey.wasPressedThisFrame || pressedHotbarKey) stepCompleted = true;
                break;
        }

        // Si pulsó la tecla correcta, cerramos este tutorial específico
        if (stepCompleted)
        {
            CompleteTutorial(activeTutorial);
        }
    }

    // --- FUNCIÓN PÚBLICA PARA LLAMAR A LOS TUTORIALES DESDE CUALQUIER SCRIPT ---
    public void ShowTutorial(int tutorialIndex)
    {
        // Comprobamos si ESTE tutorial en concreto ya lo hicimos en el pasado
        if (PlayerPrefs.GetInt("Tutorial_" + tutorialIndex, 0) == 1) return;

        activeTutorial = tutorialIndex;
        tutorialPanel.SetActive(true);

        // Mostramos el texto correspondiente
        switch (tutorialIndex)
        {
            case 0: tutorialText.text = "Usa las FLECHAS para moverte."; break;
            case 1: tutorialText.text = "Pulsa 'C' para interactuar con objetos y personajes."; break;
            case 2: tutorialText.text = "Pulsa 'Z' para atacar con tu arma."; break;
            case 3: tutorialText.text = "Pulsa 'TAB' para abrir el menú de inventario, misiones y opciones."; break;
            case 4: tutorialText.text = "Pulsa del '1' al '0' para usar objetos de la barra rápida (o pulsa 'C' para continuar)."; break;
        }
    }

    private void CompleteTutorial(int tutorialIndex)
    {
        // Guardamos que ESTE tutorial ya está completado
        PlayerPrefs.SetInt("Tutorial_" + tutorialIndex, 1);
        PlayerPrefs.Save();

        tutorialPanel.SetActive(false);
        activeTutorial = -1;

        // "El cuarto tiene que pasar después del primero":
        // Si acabamos de completar el tutorial 0 (Moverse), lanzamos automáticamente el 3 (Menú)
        if (tutorialIndex == 0)
        {
            ShowTutorial(3);
        }
    }

    [ContextMenu("Resetear Todos los Tutoriales")]
    public void ResetAllTutorials()
    {
        for (int i = 0; i < 5; i++)
        {
            PlayerPrefs.SetInt("Tutorial_" + i, 0);
        }
        PlayerPrefs.Save();
        Debug.Log("Todos los tutoriales han sido reseteados.");
    }
}