using UnityEngine;
using TMPro;

// Este script se usa exclusivamente en la última pantalla del juego.
// Lee los puntos guardados en la memoria del ordenador y muestra un texto u otro 
// dependiendo de lo rápido y habilidoso que haya sido el jugador.
public class EndScreenManager : MonoBehaviour
{
    [Header("Componentes de UI")]
    public TextMeshProUGUI finalScoreText; // El texto principal de "Has conseguido X puntos"
    public TextMeshProUGUI rankText;       // El texto dinámico de valoración ("Excelente", "Bien", etc.)

    void Start()
    {
        // 1. Buscamos en el disco duro (PlayerPrefs) la puntuación exacta 
        // que guardamos un segundo antes en el nivel anterior.
        int score = PlayerPrefs.GetInt("FinalScore", 0);

        // 2. Imprimimos el número total de puntos en la pantalla.
        if (finalScoreText != null)
        {
            finalScoreText.text = "Has conseguido " + score + " puntos";
        }

        // 3. Sistema de rangos: Evaluamos la puntuación del jugador.
        // Dependiendo de si sus puntos alcanzan ciertas barreras, 
        // le mostramos un título con un color especial usando etiquetas <color>.
        if (rankText != null)
        {
            if (score >= 3500)
            {
                // Rango Máximo (Color Dorado)
                rankText.text = "VALORACIÓN:\n<color=#FFD700>¡Excelente!</color>\nEres un auténtico maestro de la torre.";
            }
            else if (score >= 2500)
            {
                // Rango Alto (Color Verde)
                rankText.text = "VALORACIÓN:\n<color=#00FF00>¡Genial!</color>\n¡Has demostrado una velocidad increíble!";
            }
            else if (score >= 1500)
            {
                // Rango Medio (Color Cian)
                rankText.text = "VALORACIÓN:\n<color=#00FFFF>¡Bien hecho!</color>\nBuen intento, pero puedes optimizar tus tiempos.";
            }
            else
            {
                // Rango Bajo (Color Blanco)
                rankText.text = "VALORACIÓN:\n<color=#FFFFFF>¡Completado!</color>\nHas superado la demo, ¡pero intenta mejorar tu marca!";
            }
        }
    }
}