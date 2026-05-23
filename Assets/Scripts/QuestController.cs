using System.Collections.Generic;
using UnityEngine;

// Este es el Gestor General de Misiones (Singleton).
// Controla qué encargos tiene el jugador activos, revisa su mochila para ver 
// si ha recogido los objetos que le piden, y entrega las misiones completadas.
public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }

    // Lista de misiones que el jugador está intentando completar ahora mismo
    public List<QuestProgress> activateQuests = new();

    private QuestUI questUI; // Referencia a la pantalla del diario de misiones

    // Lista de DNIs de misiones que el jugador ya ha completado y cobrado
    public List<string> handInQuestsIDs = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        questUI = FindObjectOfType<QuestUI>();

        // CONEXIÓN VITAL: Le decimos al Inventario que, CADA VEZ que entre o salga 
        // un objeto de la mochila, nos avise para revisar si hemos cumplido algún objetivo.
        InventoryController.Instance.OnInventoryChanged += CheckInventoryForQuests;
    }

    // El jugador acepta un encargo nuevo
    public void AcceptQuest(Quest quest)
    {
        // Si ya tenemos esta misión, ignoramos la orden para no duplicarla
        if (IsQuestActive(quest.questID)) return;

        // La añadimos a nuestra lista de tareas activas
        activateQuests.Add(new QuestProgress(quest));

        // Por si acaso el jugador ya tenía los objetos en su mochila antes de hablar con el NPC
        CheckInventoryForQuests();
        questUI.UpdateQuestUI();

        if (questUI != null)
        {
            questUI.ShowQuestNotification($"¡Encargo aceptado:\n{quest.questName}!");
        }
    }

    public bool IsQuestActive(string questID) => activateQuests.Exists(q => q.quest.questID == questID);

    // --- REVISIÓN AUTOMÁTICA DEL INVENTARIO ---
    public void CheckInventoryForQuests()
    {
        // Le pedimos a la mochila un resumen de qué tiene y en qué cantidad
        Dictionary<int, int> itemCounts = InventoryController.Instance.GetItemCounts();

        // Repasamos todas nuestras misiones activas...
        foreach (QuestProgress quest in activateQuests)
        {
            // Y todos los objetivos de cada misión...
            foreach (QuestObjective questObjective in quest.objectives)
            {
                // Solo nos interesan los objetivos de "Recolectar Objetos"
                if (questObjective.type != ObjectiveType.CollectItem) continue;
                if (!int.TryParse(questObjective.objectiveID, out int itemID)) continue;

                // Miramos cuántos de esos objetos pide la misión y cuántos tenemos en la mochila
                int newAmount = itemCounts.TryGetValue(itemID, out int count) ? Mathf.Min(count, questObjective.requiredAmount) : 0;

                // Si ha cambiado la cantidad, actualizamos el progreso del diario
                if (questObjective.currentAmount != newAmount)
                {
                    questObjective.currentAmount = newAmount;
                }
            }
        }
        // Refrescamos la pantalla para que el jugador vea los nuevos números
        questUI.UpdateQuestUI();
    }

    // Comprueba si hemos cumplido todas las tareas de un encargo
    public bool IsQuestCompleted(string questID)
    {
        QuestProgress quest = activateQuests.Find(q => q.quest.questID == questID);
        return quest != null && quest.objectives.TrueForAll(o => o.IsCompleted);
    }

    // Entregar la misión al NPC (Cobrarla)
    public void HandInQuest(string questID)
    {
        // Primero intentamos cobrarle al jugador los objetos que pide la misión.
        // Si no tiene los objetos, no puede entregarla.
        if (!RemoveRequiredItemsFromInventory(questID))
        {
            return;
        }

        QuestProgress quest = activateQuests.Find(q => q.quest.questID == questID);
        if (quest != null)
        {
            // La marcamos en el archivo de misiones "Cobradas"
            handInQuestsIDs.Add(questID);
            // La borramos de nuestra lista de tareas activas
            activateQuests.Remove(quest);
            questUI.UpdateQuestUI();

            if (questUI != null)
            {
                questUI.ShowQuestNotification($"¡Encargo completado:\n{quest.quest.questName}!");
            }
        }
    }

    public bool IsQuestHandedIn(string questID)
    {
        return handInQuestsIDs.Contains(questID);
    }

    // Quita los objetos de la mochila para dárselos al NPC
    public bool RemoveRequiredItemsFromInventory(string questID)
    {
        QuestProgress quest = activateQuests.Find(q => q.quest.questID == questID);
        if (quest == null) return false;

        Dictionary<int, int> requiredItems = new();

        // 1. Hacemos una lista de la compra con lo que necesitamos entregar
        foreach (QuestObjective objective in quest.objectives)
        {
            if (objective.type == ObjectiveType.CollectItem && int.TryParse(objective.objectiveID, out int itemID))
            {
                requiredItems[itemID] = objective.requiredAmount;
            }
        }

        Dictionary<int, int> itemCounts = InventoryController.Instance.GetItemCounts();

        // 2. Comprobamos de nuevo si de verdad tenemos suficiente de todo
        foreach (var item in requiredItems)
        {
            if (itemCounts.GetValueOrDefault(item.Key) < item.Value)
            {
                return false; // Si nos falta aunque sea un objeto, cancelamos el cobro
            }
        }

        // 3. Si tenemos todo, le pedimos al inventario que destruya esos objetos (se los damos al NPC)
        foreach (var itemrequiremtent in requiredItems)
        {
            InventoryController.Instance.RemoveItemsFromInventory(itemrequiremtent.Key, itemrequiremtent.Value);
        }

        return true;
    }

    // Desempaqueta las misiones al cargar la partida
    public void LoadQuestProgress(List<QuestProgress> savedQuests)
    {
        activateQuests = savedQuests ?? new();
        CheckInventoryForQuests();
        questUI.UpdateQuestUI();
    }
}