using System.Collections.Generic;
using UnityEngine;

// --- 1. LA MISIÓN EN SÍ (Scriptable Object) ---
// Esto nos permite crear "fichas de misiones" directamente en nuestras carpetas de Unity 
// sin tener que programarlas una a una en la escena.
[CreateAssetMenu(menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    public string questID;       // El DNI único de la misión
    public string questName;     // Nombre (Ej: "Problemas de ratas")
    public string description;   // Historia de la misión

    public List<QuestObjective> objectives; // Lista de cosas que hay que hacer para completarla
    public List<QuestReward> questRewards;  // Lista de premios que nos darán al terminarla

    // Función de Unity que se ejecuta al modificar este archivo en el Inspector.
    private void OnValidate()
    {
        // Si a la misión le falta su DNI, le generamos uno automático e irrepetible (GUID).
        // Así el diseñador del juego no tiene que inventarse números a mano.
        if (string.IsNullOrEmpty(questID))
        {
            questID = System.Guid.NewGuid().ToString();
        }
    }
}

// --- 2. LOS OBJETIVOS DE LA MISIÓN ---
[System.Serializable]
public class QuestObjective
{
    public string objectiveID;   // Qué hay que buscar (ej: la ID del objeto o enemigo)
    public string description;   // Texto para el jugador (Ej: "Consigue 3 Manzanas")
    public ObjectiveType type;   // Qué tipo de tarea es (Recolectar, Matar, etc.)
    public int requiredAmount;   // Cuántos necesitamos
    public int currentAmount;    // Cuántos llevamos ahora mismo

    // Una comprobación rápida que responde True o False: ¿Llevamos los mismos o más de los que piden?
    public bool IsCompleted => currentAmount >= requiredAmount;
}

// Lista cerrada de tipos de tarea que existen en nuestro juego
public enum ObjectiveType { CollectItem, DefeatEnemy, ReachLocation, TalkNPC, Custom }

// --- 3. EL DIARIO DEL JUGADOR (Progreso) ---
// Esta clase representa la copia personal que se lleva el jugador en su diario 
// cuando acepta una misión. Lleva la cuenta de su progreso individual.
[System.Serializable]
public class QuestProgress
{
    public Quest quest;
    public List<QuestObjective> objectives;

    // Al aceptar la misión, hacemos una copia limpia de los objetivos con los contadores a 0.
    public QuestProgress(Quest quest)
    {
        this.quest = quest;
        objectives = new List<QuestObjective>();

        foreach (var obj in quest.objectives)
        {
            objectives.Add(new QuestObjective
            {
                objectiveID = obj.objectiveID,
                description = obj.description,
                type = obj.type,
                requiredAmount = obj.requiredAmount,
                currentAmount = 0
            });
        }
    }

    // La misión entera está completa si TODOS sus objetivos están completados.
    public bool IsCompleted() => objectives.TrueForAll(o => o.IsCompleted);
    public string QuestID => quest.questID;
}

// --- 4. LOS PREMIOS DE LA MISIÓN ---
[System.Serializable]
public class QuestReward
{
    public RewardType type; // Qué tipo de premio es (Objeto, Oro, Experiencia)
    public int rewardID;    // Qué objeto exactamente nos van a dar (ID del Catálogo)
    public int amount = 1;  // Cantidad del premio
}

public enum RewardType { Item, Gold, Experience, Custom }