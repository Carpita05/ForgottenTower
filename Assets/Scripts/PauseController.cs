using UnityEngine;

// Un script global diminuto pero importantísimo.
// Controla el estado del tiempo del juego entero para poder pausarlo y reanudarlo.
public class PauseController : MonoBehaviour
{
    // Una variable estática que cualquier script puede consultar para saber si el juego está pausado
    public static bool IsGamePaused { get; private set; } = false;

    // La función mágica que congela el universo del juego
    public static void SetPause(bool pause)
    {
        IsGamePaused = pause;

        if (pause)
        {
            // Time.timeScale a 0 significa que el tiempo se detiene por completo.
            // Las físicas paran, los enemigos no se mueven, pero las interfaces de usuario (UI) siguen funcionando.
            Time.timeScale = 0f;
        }
        else
        {
            // Time.timeScale a 1 devuelve el tiempo a su velocidad normal y todo sigue su curso.
            Time.timeScale = 1f;
        }
    }
}