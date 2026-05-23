using UnityEngine;

// ¡Esto es un ScriptableObject! No se le pone a los objetos en la escena, 
// sino que nos permite crear "archivos de guion" directamente en las carpetas de Unity.
// Funciona como una plantilla de datos pura.
[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]
public class NPCDialog : ScriptableObject
{
    [Header("Datos del Personaje")]
    public string npcName;       // Nombre del personaje
    public Sprite npcPortrait;   // Su foto o avatar para la interfaz

    [Header("Guion")]
    public string[] dialogueLines;     // Lista con todas las frases que va a decir
    public bool[] autoProgressLines;   // ¿Esta frase salta sola a la siguiente sin que el jugador pulse nada?
    public bool[] endDialoguesLines;   // ¿Esta frase obliga a cerrar la conversación y despedirse?
    public float autoProgressDelay = 1.5f; // Cuánto espera antes de saltar sola

    [Header("Efectos de Voz y Texto")]
    public float typingSpeed = 0.05f;  // Velocidad a la que aparecen las letras
    public AudioClip voiceSound;       // El "blablabla" que suena al hablar (estilo Animal Crossing)
    public float voicePitch = 1f;      // Tono de la voz (más agudo o más grave)

    [Range(0f, 1f)]
    public float voiceVolume = 0.5f;

    [Header("Interactividad y Misiones")]
    public DialogueChoice[] choices;   // Lista de opciones de respuesta que le daremos al jugador

    public int questInProgressIndex;   // Si el jugador está haciendo nuestra misión, saltamos a esta frase
    public int questCompletedIndex;    // Si el jugador ya ha terminado la misión, saltamos a esta otra
    public Quest quest;                // La misión física asociada a esta conversación
}

// Estructura auxiliar para guardar los datos de las "Opciones de Respuesta"
[System.Serializable]
public class DialogueChoice
{
    public int dialogueIndex;         // En qué número de frase deben aparecer los botones
    public string[] choices;          // El texto de los botones ("Sí", "No", "Atacar")
    public int[] nextDialogueIndexes; // A qué frase saltamos dependiendo del botón pulsado
    public bool[] giveQuest;          // ¿Este botón activa y te entrega la misión?
}