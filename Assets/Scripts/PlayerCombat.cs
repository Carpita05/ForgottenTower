using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("Configuración de Ataque")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public float attackOffset = 0.6f;
    public LayerMask enemyLayers;

    [Header("Estadísticas")]
    public int baseAttackDamage = 1;
    public int attackDamage = 1;

    public float attackRate = 2f;
    public float attackDelay = 0.25f;

    private float nextAttackTime = 0f;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Si el daño actual es menor que el base (ej. al empezar una partida nueva), lo igualamos
        if (attackDamage < baseAttackDamage)
        {
            attackDamage = baseAttackDamage;
        }
    }

    void Update()
    {
        float x = animator.GetFloat("inputX");
        float y = animator.GetFloat("inputY");

        if (x == 0 && y == 0)
        {
            x = animator.GetFloat("lastInputX");
            y = animator.GetFloat("lastInputY");
        }

        if (x == 0 && y == 0)
        {
            y = -1f;
        }

        Vector2 direction = new Vector2(x, y).normalized;
        attackPoint.localPosition = direction * attackOffset;

        animator.SetFloat("lastInputX", direction.x);
        animator.SetFloat("lastInputY", direction.y);
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed && Time.time >= nextAttackTime && !PauseController.IsGamePaused)
        {
            PerformAttack();
            nextAttackTime = Time.time + 1f / attackRate;
        }
    }

    void PerformAttack()
    {
        animator.SetTrigger("Attack");
        SoundEffectManager.Play("SwordSwing");
        StartCoroutine(DealDamageCoroutine());
    }

    private IEnumerator DealDamageCoroutine()
    {
        yield return new WaitForSeconds(attackDelay);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(attackDamage, transform);
            }

            BossHealth boss = enemy.GetComponent<BossHealth>();
            if (boss != null)
            {
                boss.TakeDamage(attackDamage, transform);
            }
        }
    }
    public void IncreaseDamage(int amount)
    {
        attackDamage += amount;
        Debug.Log("¡Poción consumida! Daño en esta partida: " + attackDamage);
    }

    public void LoadDamage(int savedDamage)
    {
        attackDamage = savedDamage;
        Debug.Log("Daño cargado desde el archivo JSON: " + attackDamage);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}