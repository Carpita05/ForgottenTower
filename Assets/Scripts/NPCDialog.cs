using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]
public class NPCDialog : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public string[] dialogueLines;
    public bool[] autoProgressLines;
    public bool[] endDialoguesLines;
    public float autoProgressDelay = 1.5f;

    public float typingSpeed = 0.05f; // Time between each character being displayed
    public AudioClip voiceSound;
    public float voicePitch = 1f;

    [Range(0f, 1f)]
    public float voiceVolume = 0.5f;

    public DialogueChoice[] choices; // Array of dialogue choices

    public int questInProgressIndex;
    public int questCompletedIndex;
    public Quest quest;
}

[System.Serializable]
public class DialogueChoice
{
    public int dialogueIndex;
    public string[] choices;
    public int[] nextDialogueIndexes; // Indices corresponding to each choice
    public bool[] giveQuest;
}