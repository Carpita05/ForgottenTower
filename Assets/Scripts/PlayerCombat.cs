using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

// Este script controla los ataques del jugador.
// Calcula hacia dónde estamos mirando, reproduce la animación del espadazo 
// y aplica el daño a los enemigos que estén dentro de nuestro rango.
public class PlayerCombat : MonoBehaviour
{
    [Header("Configuración de Ataque")]
    public Transform attackPoint;    // El punto invisible desde donde sale el golpe
    public float attackRange = 0.5f; // Lo grande que es el área del corte
    public float attackOffset = 0.6f;// A qué distancia del jugador se coloca el punto de ataque
    public LayerMask enemyLayers;    // Filtro para saber a qué capas (enemigos) podemos pegar

    [Header("Estadísticas")]
    public int baseAttackDamage = 1; // El daño normal del jugador
    public int attackDamage = 1;     // El daño actual (puede subir si tomamos pociones)

    public float attackRate = 2f;    // Cuántos ataques podemos hacer por segundo
    public float attackDelay = 0.25f;// El retraso para que el golpe encaje justo con la animación

    private float nextAttackTime = 0f; // Cronómetro interno para medir cuándo podemos volver a atacar
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Medida de seguridad: Si al cargar la partida el daño es menor al básico, lo arreglamos.
        if (attackDamage < baseAttackDamage)
        {
            attackDamage = baseAttackDamage;
        }
    }

    void Update()
    {
        // 1. Leemos hacia dónde se está moviendo el jugador a través de su animación
        float x = animator.GetFloat("inputX");
        float y = animator.GetFloat("inputY");

        // 2. Si estamos quietos, leemos hacia dónde fue nuestro último paso
        if (x == 0 && y == 0)
        {
            x = animator.GetFloat("lastInputX");
            y = animator.GetFloat("lastInputY");
        }

        // 3. Por defecto, si acabamos de empezar a jugar, miramos hacia abajo
        if (x == 0 && y == 0)
        {
            y = -1f;
        }

        // 4. Movemos el punto invisible de ataque para que siempre esté frente a nosotros
        Vector2 direction = new Vector2(x, y).normalized;
        attackPoint.localPosition = direction * attackOffset;

        // Actualizamos la memoria de la animación
        animator.SetFloat("lastInputX", direction.x);
        animator.SetFloat("lastInputY", direction.y);
    }

    // Función que se activa cuando el jugador pulsa el botón de atacar
    public void Attack(InputAction.CallbackContext context)
    {
        // Comprobamos que el botón se ha pulsado, que ha pasado el tiempo suficiente desde el último ataque 
        // y que el juego no está pausado (para no atacar mientras estamos en un menú).
        if (context.performed && Time.time >= nextAttackTime && !PauseController.IsGamePaused)
        {
            PerformAttack();
            // Calculamos cuándo podremos dar el siguiente golpe
            nextAttackTime = Time.time + 1f / attackRate;
        }
    }

    void PerformAttack()
    {
        // Disparamos la animación del espadazo y el sonido
        animator.SetTrigger("Attack");
        SoundEffectManager.Play("SwordSwing");

        // Arrancamos una secuencia especial para aplicar el daño con un pequeño retraso
        StartCoroutine(DealDamageCoroutine());
    }

    private IEnumerator DealDamageCoroutine()
    {
        // Esperamos una fracción de segundo para que el filo de la espada visualmente toque al enemigo
        yield return new WaitForSeconds(attackDelay);

        // Dibujamos un círculo invisible de daño y detectamos a todos los enemigos que estén dentro
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Repartimos daño a todo lo que hayamos tocado
        foreach (Collider2D enemy in hitEnemies)
        {
            // ¿Es un enemigo normal?
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(attackDamage, transform);
            }

            // ¿O es un jefe?
            BossHealth boss = enemy.GetComponent<BossHealth>();
            if (boss != null)
            {
                boss.TakeDamage(attackDamage, transform);
            }
        }
    }

    // Mejora el daño del jugador (Ej: Al tomar una Poción de Fuerza)
    public void IncreaseDamage(int amount)
    {
        attackDamage += amount;
        Debug.Log("¡Poción consumida! Daño actual: " + attackDamage);
    }

    // Carga el daño guardado en la memoria de la partida
    public void LoadDamage(int savedDamage)
    {
        attackDamage = savedDamage;
        Debug.Log("Daño cargado desde el archivo JSON: " + attackDamage);
    }

    // Herramienta visual para el programador: Dibuja una esfera roja en Unity 
    // para que podamos ver exactamente dónde estamos pegando.
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}