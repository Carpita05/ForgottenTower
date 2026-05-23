using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// Este script es un gestor general. En esta versión de la demo, 
// se encarga de dirigir la pantalla de Game Over y los reinicios del nivel.
public class GameManager : MonoBehaviour
{
    [Header("Interfaz de Game Over")]
    public GameObject gameOverPanel;        // La pantalla completa de "Has Muerto"
    public CanvasGroup gameOverCanvasGroup; // Herramienta para modificar la transparencia del panel entero
    public float fadeDuration = 1.5f;       // Cuánto tardan las letras en aparecer en pantalla

    // Esta función la llaman otros scripts (como PlayerHealth) cuando el jugador se queda sin vida.
    public void ShowGameOver()
    {
        // En lugar de encender la pantalla roja de golpe (lo cual queda muy brusco),
        // arrancamos una animación suave por código.
        StartCoroutine(FadeInGameOver());
    }

    private IEnumerator FadeInGameOver()
    {
        // 1. Encendemos el panel, pero le ponemos la transparencia (Alpha) a 0 para que sea invisible.
        gameOverPanel.SetActive(true);
        gameOverCanvasGroup.alpha = 0f;

        // 2. IMPORTANTE: Bloqueamos los botones del panel. 
        // Así evitamos que el jugador, del susto al morir, haga clic sin querer en "Reiniciar" 
        // antes de que la pantalla se haya mostrado del todo.
        gameOverCanvasGroup.interactable = false;
        gameOverCanvasGroup.blocksRaycasts = false;

        // 3. Vamos subiendo la opacidad (visibilidad) poco a poco hasta el máximo.
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            gameOverCanvasGroup.alpha = timer / fadeDuration;
            yield return null;
        }

        // 4. Una vez la pantalla es 100% visible, reactivamos los botones para que el jugador pueda elegir.
        gameOverCanvasGroup.alpha = 1f;
        gameOverCanvasGroup.interactable = true;
        gameOverCanvasGroup.blocksRaycasts = true;
    }

    // Recarga exactamente el mismo nivel en el que estamos para volver a intentarlo desde el principio.
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Envía al jugador de vuelta a la pantalla del título principal.
    public void GoToMainMenu()
    {
        Debug.Log("Cargando Menú Principal...");
        SceneManager.LoadScene("MainMenu");
    }
}