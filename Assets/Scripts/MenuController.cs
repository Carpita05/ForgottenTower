using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Este script controla el menú de pausa y de opciones durante el juego.
// Se encarga de abrirlo o cerrarlo al pulsar una tecla y de conectar las barras de volumen.
public class MenuController : MonoBehaviour
{
    [Header("Interfaz")]
    public GameObject menuCanvas; // La pantalla del menú al completo

    [Header("Sliders de Sonido (Pausa)")]
    public Slider sfxSlider;      // Barra de volumen para los efectos
    public Slider musicSlider;    // Barra de volumen para la música

    void Start()
    {
        // Al empezar a jugar, nos aseguramos de que el menú esté oculto
        menuCanvas.SetActive(false);

        // Si tenemos un gestor de sonido funcionando, le conectamos las barras de volumen
        if (SoundEffectManager.Instance != null && sfxSlider != null && musicSlider != null)
        {
            SoundEffectManager.Instance.ConnectSliders(sfxSlider, musicSlider);
        }
    }

    void Update()
    {
        // Si no hay teclado conectado, no hacemos nada
        if (Keyboard.current == null) return;

        // Si el jugador pulsa la tecla TAB (Tabulador)...
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            // Si la pantalla está fundiéndose a negro para cambiar de zona, bloqueamos el menú
            // para evitar bugs o que el jugador haga clics a ciegas.
            if (FadeTrigger.isTransitioning) return;

            // Si el juego ya está pausado por otra cosa (como un diálogo), no abrimos este menú
            if (!menuCanvas.activeSelf && PauseController.IsGamePaused)
            {
                return;
            }

            // Alternamos el menú: si estaba abierto se cierra, y si estaba cerrado se abre
            menuCanvas.SetActive(!menuCanvas.activeSelf);

            // Pausamos o despausamos el juego globalmente dependiendo de si el menú está abierto
            PauseController.SetPause(menuCanvas.activeSelf);

            // Si acabamos de abrir el menú, volvemos a conectar las barras de volumen por seguridad
            if (menuCanvas.activeSelf && SoundEffectManager.Instance != null && sfxSlider != null && musicSlider != null)
            {
                SoundEffectManager.Instance.ConnectSliders(sfxSlider, musicSlider);
            }
        }
    }
}