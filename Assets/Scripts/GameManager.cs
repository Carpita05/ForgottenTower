using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Necesario para las corrutinas

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject gameOverPanel;
    public CanvasGroup gameOverCanvasGroup; // NUEVO: Para controlar la transparencia
    public float fadeDuration = 1.5f;       // NUEVO: Cuánto tarda en aparecer (en segundos)

    public void ShowGameOver()
    {
        // En lugar de encenderlo de golpe, iniciamos el fundido
        StartCoroutine(FadeInGameOver());
    }

    private IEnumerator FadeInGameOver()
    {
        // 1. Encendemos el panel, pero lo hacemos totalmente invisible
        gameOverPanel.SetActive(true);
        gameOverCanvasGroup.alpha = 0f;

        // 2. Bloqueamos los clics para que el jugador no pueda pulsar "Continuar" sin querer mientras aparece
        gameOverCanvasGroup.interactable = false;
        gameOverCanvasGroup.blocksRaycasts = false;

        // 3. Hacemos el fundido poco a poco
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // Calculamos el porcentaje de transparencia (de 0 a 1)
            gameOverCanvasGroup.alpha = timer / fadeDuration;
            yield return null; // Esperamos al siguiente frame
        }

        // 4. Aseguramos que se quede al 100% visible y reactivamos los botones
        gameOverCanvasGroup.alpha = 1f;
        gameOverCanvasGroup.interactable = true;
        gameOverCanvasGroup.blocksRaycasts = true;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Debug.Log("Cargando Menú Principal...");
        SceneManager.LoadScene("MainMenu"); 
    }
}