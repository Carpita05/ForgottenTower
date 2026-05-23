using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Este script convierte a un monigote de la escena en un Personaje interactivo.
// Lee el guion (NPCDialog), abre la ventana de texto, escribe letra a letra y gestiona sus misiones.
public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialog dialogueData;         // El archivo de guion que le hemos asignado
    private DialogueController dialogueUI; // Referencia a la ventana gráfica de la pantalla

    private int dialogueIndex;             // Por qué número de frase vamos
    private bool isTyping, isDialogueActive;

    // Los 3 estados posibles de una misión
    private enum QuestState { NotStarted, InProgress, Completed }
    private QuestState questState = QuestState.NotStarted;

    private void Start()
    {
        dialogueUI = DialogueController.Instance;
    }

    // Comprueba si podemos hablar con él (solo si no estamos hablando ya)
    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    // Función que se dispara cuando el jugador se acerca y pulsa el botón de interactuar
    public void Interact()
    {
        if (dialogueData == null || (PauseController.IsGamePaused && !isDialogueActive))
        {
            Debug.LogWarning("Este NPC no tiene ningún guion asignado.");
            return;
        }

        // Si ya estamos hablando, pulsarlo sirve para pasar a la siguiente frase
        if (isDialogueActive)
        {
            NextLine();
        }
        // Si no, empezamos la conversación desde cero
        else
        {
            StartDialogue();
        }
    }

    // Arranca el motor de la conversación
    void StartDialogue()
    {
        // 1. Revisamos cómo va el jugador con nuestra misión (si tenemos una)
        SyncQuestState();

        // 2. Elegimos por qué frase empezar a leer el guion dependiendo de la misión
        if (questState == QuestState.NotStarted)
        {
            dialogueIndex = 0; // Frase inicial normal
        }
        else if (questState == QuestState.InProgress)
        {
            dialogueIndex = dialogueData.questInProgressIndex; // Frase de "¡Date prisa con mi encargo!"
        }
        else if (questState == QuestState.Completed)
        {
            dialogueIndex = dialogueData.questCompletedIndex; // Frase de "¡Gracias por la ayuda!"
        }

        isDialogueActive = true;

        // 3. Le pasamos los datos a la ventana gráfica, la encendemos y CONGELAMOS EL JUEGO
        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPortrait);
        dialogueUI.ShowDialogueUI(true);
        PauseController.SetPause(true);

        DisplayCurrentLine();
    }

    // Pregunta al Gestor de Misiones global cómo va el jugador con nuestro encargo
    private void SyncQuestState()
    {
        if (dialogueData.quest == null) return;

        string questID = dialogueData.quest.questID;

        if (QuestController.Instance.IsQuestCompleted(questID) || QuestController.Instance.IsQuestHandedIn(questID))
        {
            questState = QuestState.Completed;
        }
        else if (QuestController.Instance.IsQuestActive(questID))
        {
            questState = QuestState.InProgress;
        }
        else
        {
            questState = QuestState.NotStarted;
        }
    }

    // Lógica para saltar de una frase a otra
    void NextLine()
    {
        // Si el jugador pulsa el botón mientras el texto aún se está escribiendo letra a letra...
        if (isTyping)
        {
            // Paramos el efecto y mostramos la frase completa de golpe para los que leen rápido
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }

        dialogueUI.ClearChoices(); // Borramos botones viejos si los hubiera

        // Comprobamos si esta frase está marcada en el guion como "Frase de despedida"
        if (dialogueData.endDialoguesLines.Length > dialogueIndex && dialogueData.endDialoguesLines[dialogueIndex])
        {
            EndDialogue();
            return;
        }

        // Comprobamos si el guion dice que en esta frase debemos mostrar botones de respuesta
        foreach (DialogueChoice dialogueChoice in dialogueData.choices)
        {
            if (dialogueChoice.dialogueIndex == dialogueIndex)
            {
                DisplayChoices(dialogueChoice);
                return;
            }
        }

        // Si no hay botones ni despedidas, pasamos a la siguiente frase (si quedan)
        if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            DisplayCurrentLine();
        }
        else
        {
            EndDialogue();
        }
    }

    // El efecto visual de Máquina de Escribir
    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueUI.SetDialogueText("");

        // Cogemos la frase completa y la vamos escribiendo carácter por carácter
        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueUI.SetDialogueText(dialogueUI.dialogueText.text += letter);

            // Hacemos que suene un "bip" con cada letra para simular la voz
            SoundEffectManager.PlayVoice(dialogueData.voiceSound, dialogueData.voicePitch, dialogueData.voiceVolume);

            // Usamos Realtime porque el tiempo normal está congelado (Pausado)
            yield return new WaitForSecondsRealtime(dialogueData.typingSpeed);
        }

        isTyping = false;

        // Si la frase está configurada para saltar sola, esperamos un poco y lo hacemos
        if (dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSecondsRealtime(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    // Genera los botones físicos para que el jugador elija qué responder
    void DisplayChoices(DialogueChoice choice)
    {
        for (int i = 0; i < choice.choices.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndexes[i];
            bool givesQuest = choice.giveQuest[i];

            // Creamos un botón y le decimos qué debe ejecutar si hacen clic en él
            dialogueUI.CreateChoiceButton(choice.choices[i], () => ChooseOption(nextIndex, givesQuest));
        }
    }

    // Lo que ocurre cuando el jugador hace clic en una respuesta
    void ChooseOption(int nextIndex, bool giveQuest)
    {
        // Si esa respuesta implicaba aceptar la misión, se la damos al jugador
        if (giveQuest)
        {
            QuestController.Instance.AcceptQuest(dialogueData.quest);
            questState = QuestState.InProgress;
        }

        // Saltamos a la parte del guion que corresponda a la respuesta
        dialogueIndex = nextIndex;
        dialogueUI.ClearChoices();
        DisplayCurrentLine();
    }

    void DisplayCurrentLine()
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }

    // Cierra la charla y despide al personaje
    public void EndDialogue()
    {
        // Si la misión ya la habíamos completado pero aún no habíamos reclamado el premio... ¡nos lo da!
        if (questState == QuestState.Completed && !QuestController.Instance.IsQuestHandedIn(dialogueData.quest.questID))
        {
            HandIsQuestCompletion(dialogueData.quest);
        }

        // Limpiamos todo
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueUI.SetDialogueText("");
        dialogueUI.ShowDialogueUI(false);

        // ¡Volvemos a descongelar el tiempo para seguir jugando!
        PauseController.SetPause(false);
    }

    // Se encarga de entregar físicamente las recompensas de la misión
    public void HandIsQuestCompletion(Quest quest)
    {
        RewardController.Instance.GiveQuestReward(quest);
        QuestController.Instance.HandInQuest(quest.questID);
    }
}