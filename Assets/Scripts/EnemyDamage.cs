using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [Header("Configuración de Daño")]
    public int damageToDeal = 1;

    // 1. Detecta si chocan de forma sólida (como muros)
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            DealDamage(collision.gameObject);
        }
    }

    // 2. Detecta si se atraviesan (si alguno tiene "Is Trigger" marcado)
    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            DealDamage(collider.gameObject);
        }
    }

    // Función auxiliar para no repetir código
    private void DealDamage(GameObject playerObj)
    {
        PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageToDeal);
        }
    }
}