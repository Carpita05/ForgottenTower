using UnityEngine;
using UnityEngine.SceneManagement; // Súper importante para poder viajar entre escenas

public class ReturnToMenu : MonoBehaviour
{
    [Header("Configuración")]
    // Pon aquí el nombre exacto de tu escena del menú (ej: "MainMenu" o "MenuScene")
    public string mainMenuSceneName = "MainMenu";

    // Esta es la función que llamará nuestro botón
    public void GoToMainMenu()
    {
        // 1. Descongelamos el juego ANTES de cambiar de escena
        // Si no hacemos esto, el menú principal cargaría con el tiempo detenido en 0
        Time.timeScale = 1f;
        PauseController.SetPause(false);

        // 2. Cargamos la escena del Menú Principal
        SceneManager.LoadScene(mainMenuSceneName);
    }
}