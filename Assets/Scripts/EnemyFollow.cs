using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    [Header("Configuración")]
    public float moveSpeed = 2f;
    public float detectionRange = 5f; // Distancia para empezar a seguir

    [Header("Estado Interno")]
    public bool canMove = false;

    [Header("Configuración Visual")]
    public bool spriteMirandoALaIzquierda = true; // NUEVO: Configuración universal para cualquier asset

    private Transform player;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    void Start()
    {
        // Busca automáticamente al objeto que tenga la etiqueta "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // Calculamos la distancia al jugador
        float distance = Vector2.Distance(transform.position, player.position);

        // Si el jugador está dentro del círculo de visión...
        if (distance <= detectionRange)
        {
            // Siempre activamos la animación si está en rango para que intente saltar
            if (anim != null) anim.SetBool("isMoving", true);

            // Solo avanza físicamente si la animación ha dado permiso
            if (canMove)
            {
                // Calculamos la nueva posición un pasito hacia el jugador
                Vector2 newPos = Vector2.MoveTowards(rb.position, player.position, moveSpeed * Time.fixedDeltaTime);
                rb.MovePosition(newPos);
            }

            // --- NUEVO SISTEMA UNIVERSAL ---
            if (player.position.x > transform.position.x) // Si el jugador está a la derecha
            {
                // Volteamos solo si el sprite original miraba a la izquierda
                spriteRenderer.flipX = spriteMirandoALaIzquierda;
            }
            else if (player.position.x < transform.position.x) // Si el jugador está a la izquierda
            {
                // Hacemos lo contrario
                spriteRenderer.flipX = !spriteMirandoALaIzquierda;
            }
        }
        else
        {
            // Si está demasiado lejos, se queda quieto
            if (anim != null) anim.SetBool("isMoving", false);
        }
    }

    public void EnableMovement()
    {
        canMove = true;
    }

    public void DisableMovement()
    {
        canMove = false;
    }

    // Para ver el círculo amarillo del rango de visión en la escena
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}