using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Componentes de UI")]
    public TextMeshProUGUI scoreText;

    private int currentScore = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Si quieres que los puntos se mantengan si cambias de nivel, descomenta la línea de abajo:
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateScoreUI();
    }

    // Función global para añadir puntos desde cualquier enemigo
    public void AddPoints(int points)
    {
        currentScore += points;
        UpdateScoreUI();
        Debug.Log("¡Puntos añadidos! +" + points + " | Total: " + currentScore);
    }

    // Actualiza el texto formateándolo a 6 dígitos (ej: 000150)
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "PTS: " + currentScore.ToString("D6");
        }
    }
    public void SaveFinalScore()
    {
        PlayerPrefs.SetInt("FinalScore", currentScore);
        PlayerPrefs.Save();
        Debug.Log("Puntuación final guardada en memoria: " + currentScore);
    }
}