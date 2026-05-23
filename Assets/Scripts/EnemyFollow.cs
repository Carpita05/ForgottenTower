using UnityEngine;

// Este script es el "cerebro" del enemigo. Hace que detecte al jugador 
// si se acerca demasiado, camine hacia él, y gire su dibujo para mirarle a la cara.
public class EnemyFollow : MonoBehaviour
{
    [Header("Configuración")]
    public float moveSpeed = 2f;      // Lo rápido que camina al perseguirnos
    public float detectionRange = 5f; // A cuántos metros de distancia nos puede ver

    [Header("Estado Interno")]
    // Un seguro interno que nos dice si el enemigo tiene permiso físico para caminar 
    // (se suele apagar cuando recibe daño o está atacando).
    public bool canMove = false;

    [Header("Configuración Visual")]
    // ¿Hacia dónde mira el dibujo (sprite) original del enemigo por defecto? 
    // Esto es vital para que al girarlo no parezca que camina hacia atrás (haciendo el "moonwalk").
    public bool spriteMirandoALaIzquierda = true;

    private Transform player;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    void Start()
    {
        // Al nacer, el enemigo busca automáticamente por todo el mapa 
        // dónde está el jugador usando su etiqueta "Player".
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    // Usamos FixedUpdate en lugar de Update porque estamos moviendo el cuerpo físico (Rigidbody),
    // y esto garantiza que el movimiento sea fluido y no tiemble en pantallas de distintos hercios.
    void FixedUpdate()
    {
        // Si no hay jugador (o si ha muerto), el enemigo no hace nada.
        if (player == null) return;

        // Calculamos a qué distancia exacta estamos del jugador
        float distance = Vector2.Distance(transform.position, player.position);

        // Si el jugador entra dentro de nuestro círculo de visión...
        if (distance <= detectionRange)
        {
            // Le decimos al animador que mueva las piernas
            if (anim != null) anim.SetBool("isMoving", true);

            // Si tiene permiso para caminar...
            if (canMove)
            {
                // Calculamos matemáticamente qué camino debe tomar para dar un paso directo hacia el jugador
                Vector2 newPos = Vector2.MoveTowards(rb.position, player.position, moveSpeed * Time.fixedDeltaTime);
                rb.MovePosition(newPos); // Movemos su cuerpo físico
            }

            // --- SISTEMA UNIVERSAL PARA GIRAR LA MIRADA ---
            // Si el jugador está a nuestra derecha
            if (player.position.x > transform.position.x)
            {
                // Volteamos el dibujo solo si el diseño original miraba a la izquierda
                spriteRenderer.flipX = spriteMirandoALaIzquierda;
            }
            // Si el jugador está a nuestra izquierda
            else if (player.position.x < transform.position.x)
            {
                // Hacemos lo contrario para que le mire de frente
                spriteRenderer.flipX = !spriteMirandoALaIzquierda;
            }
        }
        else
        {
            // Si el jugador se aleja demasiado, el enemigo se queda quieto y para la animación.
            if (anim != null) anim.SetBool("isMoving", false);
        }
    }

    // Funciones que se llaman desde las animaciones para permitirle o prohibirle caminar.
    public void EnableMovement()
    {
        canMove = true;
    }

    public void DisableMovement()
    {
        canMove = false;
    }

    // Esta función dibuja un círculo amarillo invisible en la pantalla de Unity. 
    // Ayuda muchísimo al programador a ver exactamente hasta dónde llega la visión del monstruo.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}