using UnityEngine;

// Este script es muy sencillo: hace que el enemigo quite vida al jugador
// simplemente con tocarlo o rozarlo.
public class EnemyDamage : MonoBehaviour
{
    [Header("Configuración de Daño")]
    public int damageToDeal = 1; // Cuánta vida quita por cada golpe

    // 1er Caso: Si el enemigo y el jugador chocan como si fueran objetos sólidos 
    // (como chocarse contra un muro de ladrillos).
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            DealDamage(collision.gameObject);
        }
    }

    // 2do Caso: Si el enemigo y el jugador se atraviesan 
    // (por ejemplo, si el ataque es una zona mágica o un fantasma que no tiene cuerpo sólido).
    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            DealDamage(collider.gameObject);
        }
    }

    // Esta es la acción real de hacer daño. 
    // La sacamos aparte a una función propia para no tener que escribir lo mismo dos veces arriba.
    private void DealDamage(GameObject playerObj)
    {
        // Buscamos el componente de vida del jugador al que acabamos de tocar
        PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();

        // Si realmente tiene un componente de vida, le restamos el daño
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageToDeal);
        }
    }
}