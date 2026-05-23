using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoEndMenu : MonoBehaviour
{
    [Header("Configuración")]
    public string mainMenuSceneName = "MainMenu"; // Asegúrate de que coincida con el nombre de tu escena de menú

    public void ReturnToMainMenu()
    {
        // Limpiamos los datos temporales por si quiere volver a jugar
        PlayerPrefs.DeleteKey("PlayerAttackDamage");

        // Cargamos el menú principal
        SceneManager.LoadScene(mainMenuSceneName);
    }
}