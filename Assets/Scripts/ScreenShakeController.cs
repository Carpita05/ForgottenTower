using UnityEngine;
using Unity.Cinemachine;

public class ScreenShakeController : MonoBehaviour
{
    public static ScreenShakeController Instance { get; private set; }

    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    // Función para llamar desde cualquier parte del juego
    public void Shake(float force = 1f)
    {
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulseWithForce(force);
        }
    }
}