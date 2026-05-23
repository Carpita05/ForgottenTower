using UnityEngine;
using UnityEngine.InputSystem;

// Este script controla las "piernas" del jugador.
// Lee las direcciones del mando o teclado (Input System), mueve el cuerpo físico 
// y avisa al sistema de animación para que el dibujo mueva los pies.
public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float moveSpeed = 5f; // Velocidad al caminar

    private Rigidbody2D rb;
    private Vector2 moveInput; // Guarda hacia dónde queremos ir (X, Y)
    private Animator animator;

    // Un seguro que otros scripts pueden apagar para dejarnos paralizados 
    // (por ejemplo, al hablar con un NPC o al morir).
    public bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Si el juego está pausado o nos han quitado el permiso para movernos...
        if (PauseController.IsGamePaused || !canMove)
        {
            // Clavamos los frenos para no seguir resbalando como si estuviéramos en hielo
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false); // Paramos la animación de caminar
            return;
        }

        // Si podemos movernos, le damos velocidad al cuerpo físico en la dirección pulsada
        rb.linearVelocity = moveInput * moveSpeed;

        // Si nos estamos moviendo (la velocidad es mayor que 0), activamos la animación de caminar
        animator.SetBool("isWalking", rb.linearVelocity.magnitude > 0);
    }

    // Esta función la llama automáticamente el Input System de Unity al tocar las teclas (WASD/Flechas)
    public void Move(InputAction.CallbackContext context)
    {
        // Si soltamos la tecla (canceled)...
        if (context.canceled)
        {
            animator.SetBool("isWalking", false);
            // Guardamos hacia dónde dimos el último paso para que el personaje se quede mirando hacia ahí
            animator.SetFloat("lastInputX", moveInput.x);
            animator.SetFloat("lastInputY", moveInput.y);
        }

        // Leemos la dirección exacta que está pulsando el jugador
        moveInput = context.ReadValue<Vector2>();

        // Le enviamos esos datos al animador para que sepa qué dibujo cargar (mirar arriba, abajo, etc.)
        animator.SetFloat("inputX", moveInput.x);
        animator.SetFloat("inputY", moveInput.y);
    }
}