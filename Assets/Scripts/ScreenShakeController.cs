using UnityEngine;
using Unity.Cinemachine;

// Este script conecta nuestro código con el sistema avanzado de cámaras Cinemachine de Unity.
// Nos permite generar vibraciones o sacudidas (Screen Shake) en la pantalla 
// desde cualquier parte del juego con una sola línea de código, mejorando drásticamente el 'Game Feel'.
public class ScreenShakeController : MonoBehaviour
{
    public static ScreenShakeController Instance { get; private set; }

    // El emisor de ondas sísmicas de Cinemachine
    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    // Esta función la llaman otros scripts (como PlayerHealth) cuando recibimos un golpe o morimos
    public void Shake(float force = 1f)
    {
        if (impulseSource != null)
        {
            // Generamos un impacto físico simulado en la cámara con la fuerza deseada
            impulseSource.GenerateImpulseWithForce(force);
        }
    }
}