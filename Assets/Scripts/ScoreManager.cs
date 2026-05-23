using UnityEngine;
using TMPro;

// Este script (Singleton) lleva la cuenta de los puntos durante la partida.
// Actualiza la interfaz en tiempo real y guarda el resultado final en el disco duro 
// para que la Pantalla de Victoria pueda leerlo más tarde.
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Componentes de UI")]
    public TextMeshProUGUI scoreText; // El texto de la pantalla donde sale el número

    private int currentScore = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateScoreUI();
    }

    // Función pública para que los enemigos nos den puntos al morir
    public void AddPoints(int points)
    {
        currentScore += points;
        UpdateScoreUI();
        Debug.Log("¡Puntos añadidos! +" + points + " | Total: " + currentScore);
    }

    // Actualiza el texto de la pantalla dándole un formato clásico de máquina recreativa
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            // "D6" obliga al número a tener siempre 6 dígitos, rellenando con ceros a la izquierda 
            // (Ejemplo: Si tienes 150 puntos, se verá como "PTS: 000150")
            scoreText.text = "PTS: " + currentScore.ToString("D6");
        }
    }

    // Empaqueta los puntos y los manda a la memoria del PC antes de cambiar de nivel
    public void SaveFinalScore()
    {
        PlayerPrefs.SetInt("FinalScore", currentScore);
        PlayerPrefs.Save(); // Forzamos el guardado inmediato
        Debug.Log("Puntuación final guardada en memoria: " + currentScore);
    }
}