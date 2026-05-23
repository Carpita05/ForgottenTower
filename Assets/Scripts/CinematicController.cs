using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.InputSystem;

// Este script controla la pantalla del vídeo introductorio (cinemática).
// Se encarga de vigilar cuándo termina el vídeo para pasar al juego real, 
// y le da al jugador la opción de saltarse la cinemática si tiene prisa.
public class CinematicController : MonoBehaviour
{
    [Header("Configuración")]
    // El reproductor que proyecta el vídeo en la pantalla.
    public VideoPlayer videoPlayer;

    // El nombre exacto de la escena (nivel) que debe cargar al terminar el vídeo.
    public string nextSceneName = "GameScene";

    void Start()
    {
        // Medida de seguridad: Si se nos olvidó asignar el reproductor en el Inspector, 
        // el código lo busca automáticamente dentro de su propio objeto para evitar errores.
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        // Le pedimos a Unity que nos avise exactamente en el momento en que el vídeo 
        // llegue a su punto final. Cuando eso pase, disparará la función "EndReached".
        videoPlayer.loopPointReached += EndReached;
    }

    void Update()
    {
        // Si no hay ningún teclado conectado al ordenador, no hacemos nada.
        if (Keyboard.current == null) return;

        // Comprobamos constantemente si el jugador ha pulsado la tecla "Escape".
        // Si lo hace, interrumpimos la película inmediatamente.
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SkipCinematic();
        }
    }

    // Esta función funciona como una alarma: salta sola de forma automática 
    // en cuanto el vídeo reproduce su último segundo.
    void EndReached(VideoPlayer vp)
    {
        SkipCinematic();
    }

    // La función final que hace la transición de la película al juego.
    void SkipCinematic()
    {
        // Nos desconectamos de la "alarma" del vídeo. Es una buena práctica de programación 
        // para asegurar que el evento no intente ejecutarse dos veces por error.
        videoPlayer.loopPointReached -= EndReached;

        // Cargamos la pantalla de juego para empezar la partida.
        SceneManager.LoadScene(nextSceneName);
    }
}