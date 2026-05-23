using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Este script controla la ventana gráfica (UI) donde hablan los personajes.
// Es un Mánager Universal (Singleton) para que podamos enviarle textos desde cualquier lugar.
public class DialogueController : MonoBehaviour
{
    // Hacemos que este script sea accesible globalmente (Singleton).
    public static DialogueController Instance { get; private set; }

    [Header("Elementos de la Interfaz (UI)")]
    public GameObject dialoguePanel;     // El cuadro principal del diálogo
    public TMP_Text dialogueText;        // El texto donde sale la conversación
    public TMP_Text nameText;            // El texto que muestra el nombre de quién habla
    public Image portraitImage;          // La foto o cara del personaje

    [Header("Botones de Opciones")]
    public Transform choicesContainer;   // La zona donde aparecerán los botones para responder
    public GameObject choiceButtonPrefab;// El molde (Prefab) de botón de respuesta

    // Al arrancar, nos aseguramos de que solo exista un controlador de diálogo.
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Enciende o apaga la ventana entera de diálogo de la pantalla.
    public void ShowDialogueUI(bool show)
    {
        dialoguePanel.SetActive(show);
    }

    // Configura quién está hablando: actualiza su nombre y su foto.
    public void SetNPCInfo(string npcName, Sprite portrait)
    {
        nameText.SetText(npcName);
        portraitImage.sprite = portrait;
    }

    // Escribe el texto que está diciendo el personaje en ese momento.
    public void SetDialogueText(string text)
    {
        dialogueText.text = text;
    }

    // Borra todos los botones de respuesta anteriores para dejar la ventana limpia.
    public void ClearChoices()
    {
        foreach (Transform child in choicesContainer) Destroy(child.gameObject);
    }

    // Crea un botón de respuesta para el jugador (por ejemplo: "Perdonar" o "Atacar").
    // Le asigna un texto y le dice qué función debe ejecutar si el jugador hace clic en él.
    public GameObject CreateChoiceButton(string choiceText, UnityEngine.Events.UnityAction onclick)
    {
        // 1. Clonamos un botón vacío y lo metemos en la zona de respuestas.
        GameObject choiceButton = Instantiate(choiceButtonPrefab, choicesContainer);

        // 2. Le ponemos el texto correspondiente.
        choiceButton.GetComponentInChildren<TMP_Text>().text = choiceText;

        // 3. Le decimos qué tiene que hacer al ser pulsado.
        choiceButton.GetComponent<Button>().onClick.AddListener(onclick);

        return choiceButton;
    }
}