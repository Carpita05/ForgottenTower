using UnityEngine;
using TMPro;

public class EndScreenManager : MonoBehaviour
{
    [Header("Componentes de UI (TextMeshPro)")]
    public TextMeshProUGUI finalScoreText; // El texto de "Has conseguido X puntos"
    public TextMeshProUGUI rankText;       // El texto de "Excelente / Genial / Bien"

    void Start()
    {
        // 1. Leemos la puntuación final que guardamos en la escena anterior
        int score = PlayerPrefs.GetInt("FinalScore", 0);

        // 2. Mostramos el mensaje principal
        if (finalScoreText != null)
        {
            finalScoreText.text = "Has conseguido " + score + " puntos";
        }

        // 3. Evaluamos la escala de rangos según tus límites (Mín 540 - Máx 4400)
        if (rankText != null)
        {
            if (score >= 3500)
            {
                rankText.text = "VALORACIÓN:\n<color=#FFD700>¡Excelente!</color>\nEres un auténtico maestro de la torre.";
            }
            else if (score >= 2500)
            {
                rankText.text = "VALORACIÓN:\n<color=#00FF00>¡Genial!</color>\n¡Has demostrado una velocidad increíble!";
            }
            else if (score >= 1500)
            {
                rankText.text = "VALORACIÓN:\n<color=#00FFFF>¡Bien hecho!</color>\nBuen intento, pero puedes optimizar tus tiempos.";
            }
            else
            {
                rankText.text = "VALORACIÓN:\n<color=#FFFFFF>¡Completado!</color>\nHas superado la demo, ¡pero intenta mejorar tu marca!";
            }
        }
    }
}