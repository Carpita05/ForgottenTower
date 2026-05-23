using UnityEngine;

// Este script convierte un objeto del inventario en una Poción de Vida.
// Como "hereda" de la clase Item base, el inventario ya sabe cómo agarrarlo y moverlo;
// nosotros solo definimos qué pasa específicamente al usarlo.
public class HealthPotionItem : Item
{
    [Header("Poción")]
    public int healAmount = 3; // Cuánta vida recupera al beberla

    // Esta función se dispara automáticamente cuando el jugador usa el objeto 
    // (por ejemplo, desde la barra de acceso rápido o haciendo clic en el inventario).
    public override void UseItem()
    {
        // 1. Buscamos al jugador en el nivel
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();

            // 2. Comprobamos si el jugador realmente necesita curarse.
            // Si ya tiene la vida al máximo, no malgastamos la poción.
            if (health != null && health.currentHealth < health.maxHealth)
            {
                // Le curamos la cantidad indicada
                health.Heal(healAmount);

                // SoundEffectManager.Play("DrinkPotion");

                // 3. Consumimos el objeto: le restamos 1 a la cantidad acumulada (stack)
                RemoveFromStack(1);

                // 4. Le avisamos al inventario de que hemos gastado un objeto 
                // para que actualice los números en la pantalla.
                if (InventoryController.Instance != null)
                {
                    InventoryController.Instance.RebuildItemCounts();
                }

                // 5. Si nos acabamos de beber la última poción del montón, 
                // vaciamos el hueco y destruimos el objeto para que desaparezca.
                if (quantity <= 0)
                {
                    Slot mySlot = GetComponentInParent<Slot>();
                    if (mySlot != null) mySlot.currentItem = null;
                    Destroy(gameObject);
                }
            }
            else
            {
                // Damos feedback en la consola si intentó curarse estando sano
                Debug.Log("Ya tienes la vida al máximo. Poción no gastada.");
            }
        }
    }
}