using UnityEngine;
using UnityEngine.SceneManagement;

// Un script muy conciso y directo para el botón de "Volver al Menú Principal".
// Se utiliza en la pantalla de Game Over y en el menú de Pausa.
public class ReturnToMenu : MonoBehaviour
{
    [Header("Configuración")]
    // El nombre exacto de la pantalla principal a la que queremos volver
    public string mainMenuSceneName = "MainMenu";

    // Esta es la función que se ejecuta al hacer clic en el botón
    public void GoToMainMenu()
    {
        // 1. DESCONGELAR EL TIEMPO
        // Esto es absolutamente crítico. Si volvemos al menú mientras el juego estaba pausado 
        // o durante la pantalla de Game Over (donde el tiempo se detiene), 
        // el menú principal cargaría congelado. 
        Time.timeScale = 1f;
        PauseController.SetPause(false);

        // 2. CARGAR EL MENÚ
        // Una vez el flujo del tiempo vuelve a la normalidad, viajamos a la pantalla inicial
        SceneManager.LoadScene(mainMenuSceneName);
    }
}