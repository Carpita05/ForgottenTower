using Unity.Cinemachine;
using UnityEngine;

// Este script se coloca en los bordes de las pantallas (puertas).
// Cuando el jugador lo cruza, hace tres cosas: limita la cámara para que no enfoque fuera del mapa,
// empuja un poco al jugador para que no se quede atascado en la puerta, 
// y avisa al minimapa de que hemos cambiado de habitación.
public class MapTransation : MonoBehaviour
{
    [Header("Configuración de la Sala")]
    // Los límites invisibles de la NUEVA habitación a la que vamos a entrar
    [SerializeField] PolygonCollider2D mapBoundry;

    // La cámara inteligente que sigue al jugador
    CinemachineConfiner2D confiner;

    [Header("Dirección de la Puerta")]
    [SerializeField] Direction direction;
    // Cuántos metros empujamos al jugador hacia adelante para meterlo bien en la sala
    [SerializeField] float additivPos = 2f;

    // Lista de opciones para elegir hacia dónde empujar al jugador
    enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    [System.Obsolete] // Etiqueta técnica: avisa de que FindObjectOfType es un método antiguo en Unity, pero funcional
    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner2D>();
    }

    // Cuando el jugador choca contra la línea de transición (la puerta)...
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 1. Le decimos a la cámara: "Tus nuevos límites de visión son esta habitación".
            confiner.BoundingShape2D = mapBoundry;

            // 2. Empujamos al jugador hacia adentro
            UpdatePlayerPosition(collision.gameObject);

            // 3. Le avisamos al Cartógrafo (Minimapa) de que actualice la posición en el menú
            MapController_Dynamic.Instance?.UpdateCurrentArea(mapBoundry.name);
        }
    }

    // Empuja al personaje teletransportándolo ligeramente en la dirección correcta 
    // para evitar bugs de quedarse atrapado entre dos pantallas.
    private void UpdatePlayerPosition(GameObject player)
    {
        Vector3 newPos = player.transform.position;

        switch (direction)
        {
            case Direction.Up:
                newPos.y += additivPos;
                break;
            case Direction.Down:
                newPos.y -= additivPos;
                break;
            case Direction.Left:
                newPos.x += additivPos;
                break;
            case Direction.Right:
                newPos.x -= additivPos;
                break;
        }

        // Aplicamos el movimiento
        player.transform.position = newPos;
    }
}