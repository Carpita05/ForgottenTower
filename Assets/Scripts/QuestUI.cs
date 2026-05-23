using NUnit.Framework;
using System.Collections.Generic;
using System.Collections; // NUEVO: Necesario para usar Corrutinas (IEnumerator)
using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    [Header("Lista de Misiones")]
    public Transform questListContent;
    public GameObject questEntryPrefab;
    public GameObject objectiveTextPrefab;

    [Header("Notificación de Misión (Popup)")]
    public CanvasGroup notificationPanel;
    public TextMeshProUGUI notificationText;
    public float notificationDuration = 2.5f;

    void Start()
    {
        // Asegurarnos de que el panel está invisible al empezar
        if (notificationPanel != null) notificationPanel.alpha = 0f;

        UpdateQuestUI();
    }

    public void UpdateQuestUI()
    {
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var quest in QuestController.Instance.activateQuests)
        {
            GameObject entry = Instantiate(questEntryPrefab, questListContent);
            TMP_Text questNameText = entry.transform.Find("QuestName").GetComponent<TMP_Text>();
            Transform objectiveList = entry.transform.Find("ObjectiveList");

            questNameText.text = quest.quest.questName;

            foreach (var objective in quest.objectives)
            {
                GameObject objTextGO = Instantiate(objectiveTextPrefab, objectiveList);
                TMP_Text objText = objTextGO.GetComponent<TMP_Text>();
                objText.text = $"{objective.description} ({objective.currentAmount}/{objective.requiredAmount})";
            }
        }
    }

    public void ShowQuestNotification(string message)
    {
        if (notificationPanel != null && notificationText != null)
        {
            StopAllCoroutines(); // Detenemos cualquier otra notificación que estuviera a medias
            StartCoroutine(FadeNotificationCoroutine(message));
        }
    }

    private IEnumerator FadeNotificationCoroutine(string message)
    {
        notificationText.text = message;

        // 1. Fade In (Aparecer rápidamente)
        float timer = 0f;
        while (timer < 0.5f)
        {
            timer += Time.deltaTime;
            notificationPanel.alpha = timer / 0.5f;
            yield return null;
        }
        notificationPanel.alpha = 1f;

        // 2. Esperar para que el jugador lo lea
        yield return new WaitForSeconds(notificationDuration);

        // 3. Fade Out (Desaparecer lentamente)
        timer = 0f;
        while (timer < 0.8f)
        {
            timer += Time.deltaTime;
            notificationPanel.alpha = 1f - (timer / 0.8f);
            yield return null;
        }
        notificationPanel.alpha = 0f;
    }
}