using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoEndTrigger : MonoBehaviour
{
    [Header("Configuración de Escena")]
    public string endSceneName = "DemoEndScene"; // Nombre de la escena que creamos en el Paso 1

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si ya se ha activado o no es el jugador, no hacemos nada
        if (hasTriggered || !collision.CompareTag("Player")) return;

        hasTriggered = true;
        Debug.Log("¡El jugador ha llegado al final de la demo!");

        PlayerMovement movement = collision.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.canMove = false;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SaveFinalScore();
        }
        SceneManager.LoadScene(endSceneName);
    }
}