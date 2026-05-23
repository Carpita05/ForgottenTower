using System.Collections.Generic;
using System.Collections; // Necesario para usar secuencias de tiempo (IEnumerator)
using TMPro;
using UnityEngine;

// Este script controla toda la parte visual de las misiones (La Interfaz).
// Se encarga de dibujar la lista de misiones activas en el menú de pausa 
// y de mostrar los cartelitos emergentes cuando aceptamos o terminamos un encargo.
public class QuestUI : MonoBehaviour
{
    [Header("Lista de Misiones (Menú)")]
    public Transform questListContent;      // La "caja" vacía donde irán los textos
    public GameObject questEntryPrefab;     // El molde visual del título de la misión
    public GameObject objectiveTextPrefab;  // El molde visual para las tareas de la misión

    [Header("Notificación Emergente (Popup)")]
    public CanvasGroup notificationPanel;   // El panel entero de la notificación (para hacerlo transparente)
    public TextMeshProUGUI notificationText;// El texto donde dice "Misión completada"
    public float notificationDuration = 2.5f; // Cuántos segundos se queda el cartel en pantalla

    void Start()
    {
        // Al empezar el juego, nos aseguramos de que el cartel de notificación esté 100% invisible
        if (notificationPanel != null) notificationPanel.alpha = 0f;

        // Dibujamos la lista de misiones por si ya tuviéramos alguna activa al cargar partida
        UpdateQuestUI();
    }

    // --- DIBUJAR EL DIARIO DE MISIONES ---
    public void UpdateQuestUI()
    {
        // 1. Limpieza: Borramos todos los textos antiguos para no duplicarlos
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        // 2. Recreación: Leemos el Gestor de Misiones y dibujamos las misiones que tenemos a medias
        foreach (var quest in QuestController.Instance.activateQuests)
        {
            // Creamos un nuevo bloque de título para la misión
            GameObject entry = Instantiate(questEntryPrefab, questListContent);

            // Buscamos dentro de ese bloque el texto específico y le ponemos el nombre de la misión
            TMP_Text questNameText = entry.transform.Find("QuestName").GetComponent<TMP_Text>();
            questNameText.text = quest.quest.questName;

            // Buscamos la zona donde van los objetivos
            Transform objectiveList = entry.transform.Find("ObjectiveList");

            // 3. Dibujamos la lista de tareas de esa misión (Ej: "Recolectar pociones (1/3)")
            foreach (var objective in quest.objectives)
            {
                GameObject objTextGO = Instantiate(objectiveTextPrefab, objectiveList);
                TMP_Text objText = objTextGO.GetComponent<TMP_Text>();

                // Formateamos el texto para que muestre el progreso matemático
                objText.text = $"{objective.description} ({objective.currentAmount}/{objective.requiredAmount})";
            }
        }
    }

    // --- SISTEMA DE NOTIFICACIONES ---

    // Esta función la llaman otros scripts para avisar de cosas al jugador
    public void ShowQuestNotification(string message)
    {
        if (notificationPanel != null && notificationText != null)
        {
            // Detenemos cualquier otra notificación que estuviera reproduciéndose a medias
            StopAllCoroutines();
            // Arrancamos la película visual de la nueva notificación
            StartCoroutine(FadeNotificationCoroutine(message));
        }
    }

    // La "película" visual de aparición y desaparición
    private IEnumerator FadeNotificationCoroutine(string message)
    {
        notificationText.text = message;

        // FASE 1: Fade In (Aparecer rápidamente de la nada)
        float timer = 0f;
        while (timer < 0.5f)
        {
            timer += Time.deltaTime;
            notificationPanel.alpha = timer / 0.5f;
            yield return null; // Esperamos al siguiente fotograma
        }
        notificationPanel.alpha = 1f; // Lo dejamos totalmente opaco

        // FASE 2: Espera (Dejamos que el jugador lo lea tranquilamente)
        yield return new WaitForSeconds(notificationDuration);

        // FASE 3: Fade Out (Desaparecer lentamente y con suavidad)
        timer = 0f;
        while (timer < 0.8f)
        {
            timer += Time.deltaTime;
            // Aquí restamos desde 1 para que vaya volviéndose transparente
            notificationPanel.alpha = 1f - (timer / 0.8f);
            yield return null;
        }
        notificationPanel.alpha = 0f; // Lo volvemos totalmente invisible al terminar
    }
}