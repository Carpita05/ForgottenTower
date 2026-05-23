using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

// Este script guía al jugador con un tutorial paso a paso la primera vez que juega.
// Lee las teclas que pulsamos y, cuando lo completamos, usa 'PlayerPrefs' 
// para recordar que ya no tiene que volver a mostrárnoslo nunca más.
public class TutorialManager : MonoBehaviour
{
    [Header("UI del Tutorial")]
    public GameObject tutorialPanel;        // El cuadro de texto del tutorial
    public TextMeshProUGUI tutorialText;    // El texto que va cambiando

    private int currentStep = 0; // En qué paso del tutorial estamos

    void Start()
    {
        // Comprobamos en el disco duro si el jugador ya completó el tutorial en el pasado.
        // Si el valor guardado es 1 (Sí), apagamos el tutorial directamente y no hacemos nada más.
        if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 1)
        {
            tutorialPanel.SetActive(false);
            return;
        }

        // Si es la primera vez (el valor es 0), encendemos el panel y mostramos la primera instrucción
        tutorialPanel.SetActive(true);
        ShowCurrentStep();
    }

    void Update()
    {
        if (!tutorialPanel.activeSelf) return;

        // Dependiendo de en qué paso estemos, el sistema espera a que pulsemos una tecla específica
        switch (currentStep)
        {
            case 0: // Paso 1: Moverse
                if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame ||
                    Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
                {
                    NextStep();
                }
                break;

            case 1: // Paso 2: Interactuar
                if (Keyboard.current.cKey.wasPressedThisFrame) NextStep();
                break;

            case 2: // Paso 3: Atacar
                if (Keyboard.current.zKey.wasPressedThisFrame) NextStep();
                break;

            case 3: // Paso 4: Abrir el Menú
                if (Keyboard.current.tabKey.wasPressedThisFrame) NextStep();
                break;
        }
    }

    // Actualiza el texto en pantalla según el paso actual
    void ShowCurrentStep()
    {
        switch (currentStep)
        {
            case 0:
                tutorialText.text = "Usa las FLECHAS para moverte.";
                break;
            case 1:
                tutorialText.text = "Pulsa 'C' para interactuar con objetos y personajes.";
                break;
            case 2:
                tutorialText.text = "Pulsa 'Z' para atacar con tu arma.";
                break;
            case 3:
                tutorialText.text = "Pulsa 'TAB' para abrir el menú de inventario, misiones y opciones.";
                break;
        }
    }

    // Avanza al siguiente paso o termina el tutorial
    void NextStep()
    {
        currentStep++;

        if (currentStep > 3)
        {
            // Apagamos el panel
            tutorialPanel.SetActive(false);

            // Guardamos permanentemente en el ordenador que el tutorial ya está hecho
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();
        }
        else
        {
            ShowCurrentStep();
        }
    }

    // Herramienta técnica: Nos permite reiniciar el tutorial haciendo clic derecho en el script 
    // dentro del Inspector de Unity (muy útil para pruebas de desarrollo).
    [ContextMenu("Resetear Tutorial")]
    public void ResetTutorial()
    {
        PlayerPrefs.SetInt("TutorialCompleted", 0);
        PlayerPrefs.Save();
        Debug.Log("Tutorial reseteado. Vuelve a darle al Play para verlo.");
    }
}