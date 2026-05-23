using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.InputSystem;

public class CinematicController : MonoBehaviour
{
    [Header("Configuración")]
    public VideoPlayer videoPlayer;
    public string nextSceneName = "GameScene";

    void Start()
    {
        // Si no hemos asignado el VideoPlayer, lo busca automáticamente en el mismo objeto
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        // Nos suscribimos al evento "loopPointReached" que Unity dispara cuando el vídeo llega al final
        videoPlayer.loopPointReached += EndReached;
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // Si el jugador pulsa Escape, cortamos la cinemática
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SkipCinematic();
        }
    }

    // Esta función salta automáticamente cuando el vídeo termina de reproducirse
    void EndReached(VideoPlayer vp)
    {
        SkipCinematic();
    }

    void SkipCinematic()
    {
        // Limpiamos el evento por seguridad y cargamos el nivel
        videoPlayer.loopPointReached -= EndReached;
        SceneManager.LoadScene(nextSceneName);
    }
}