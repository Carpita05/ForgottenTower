using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    [Header("UI del Tutorial")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;

    private int currentStep = 0;

    void Start()
    {
        // Comprobamos si el jugador ya ha completado el tutorial antes
        // Si el valor es 1, significa que ya lo hizo, así que apagamos el panel y salimos.
        if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 1)
        {
            tutorialPanel.SetActive(false);
            return;
        }

        // Si es la primera vez, activamos el panel y mostramos el primer paso
        tutorialPanel.SetActive(true);
        ShowCurrentStep();
    }

    void Update()
    {
        // Si el tutorial está apagado, no hacemos nada
        if (!tutorialPanel.activeSelf) return;

        // Comprobamos qué tecla debe pulsar el jugador según el paso actual
        switch (currentStep)
        {
            case 0: // Paso 1: Moverse (Flechas o WASD)
                if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame ||
                    Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
                {
                    NextStep();
                }
                break;

            case 1: // Paso 2: Interactuar (C)
                if (Keyboard.current.cKey.wasPressedThisFrame)
                {
                    NextStep();
                }
                break;

            case 2: // Paso 3: Atacar (Z)
                if (Keyboard.current.zKey.wasPressedThisFrame)
                {
                    NextStep();
                }
                break;

            case 3: // NUEVO Paso 4: Abrir Menú (Tab)
                if (Keyboard.current.tabKey.wasPressedThisFrame)
                {
                    NextStep();
                }
                break;
        }
    }

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

    void NextStep()
    {
        currentStep++;

        if (currentStep > 3)
        {
            tutorialPanel.SetActive(false);

            // Guardamos que el tutorial está completado para siempre
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();
        }
        else
        {
            // Si quedan pasos, mostramos el siguiente texto
            ShowCurrentStep();
        }
    }

    // (Opcional) Función para resetear el tutorial mientras haces pruebas en Unity
    [ContextMenu("Resetear Tutorial")]
    public void ResetTutorial()
    {
        PlayerPrefs.SetInt("TutorialCompleted", 0);
        PlayerPrefs.Save();
        Debug.Log("Tutorial reseteado. Vuelve a darle al Play para verlo.");
    }
}