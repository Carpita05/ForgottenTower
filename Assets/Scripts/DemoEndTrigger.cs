using UnityEngine;
using UnityEngine.SceneManagement;

// Este script se coloca en una zona invisible al final del mapa.
// Cuando el jugador la pisa, el nivel termina: le quitamos el control, 
// guardamos sus puntos y le llevamos a la pantalla final de puntuaciones.
public class DemoEndTrigger : MonoBehaviour
{
    [Header("Configuración de Escena")]
    // El nombre exacto de la pantalla (escena) a la que iremos al terminar.
    public string endSceneName = "EndScene";

    // Un seguro para que el nivel no intente terminar dos veces a la vez.
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si el final ya se ha activado, o si ha sido otra cosa distinta al jugador 
        // quien ha pisado la zona, cancelamos la acción.
        if (hasTriggered || !collision.CompareTag("Player")) return;

        // Activamos el seguro para que esta acción solo ocurra una única vez.
        hasTriggered = true;
        Debug.Log("¡El jugador ha llegado al final de la demo!");

        // Buscamos el control de movimiento del jugador y se lo bloqueamos.
        // Así evitamos que siga corriendo o atacando mientras la pantalla cambia de nivel.
        PlayerMovement movement = collision.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.canMove = false;
        }

        // Le decimos a nuestro sistema global de puntuación que guarde los puntos 
        // en la memoria del ordenador para poder verlos en la siguiente pantalla.
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SaveFinalScore();
        }

        // Cargamos la pantalla final de victoria.
        SceneManager.LoadScene(endSceneName);
    }
}