using UnityEngine;

// Este script define un objeto consumible específico: La Poción de Fuerza.
// Hereda de la clase base "Item", por lo que ya sabe cómo apilarse y moverse por la mochila;
// aquí solo programamos qué hace de forma única al ser bebida.
public class StrengthPotionItem : Item
{
    [Header("Poción de Fuerza")]
    public int damageIncreaseAmount = 1; // Cuántos puntos de daño extra nos da

    // Reescribimos la función de uso genérica
    public override void UseItem()
    {
        // 1. Buscamos al jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerCombat combatScript = player.GetComponent<PlayerCombat>();

            if (combatScript != null)
            {
                // 2. Le subimos el daño de forma permanente para el resto de la partida
                combatScript.IncreaseDamage(damageIncreaseAmount);

                // 3. Gastamos la poción: restamos 1 a la cantidad del montón
                RemoveFromStack(1);

                // 4. Avisamos a la mochila para que actualice los números en pantalla
                if (InventoryController.Instance != null)
                {
                    InventoryController.Instance.RebuildItemCounts();
                }

                // 5. Si nos hemos bebido la última, vaciamos el hueco y borramos la imagen
                if (quantity <= 0)
                {
                    Slot mySlot = GetComponentInParent<Slot>();
                    if (mySlot != null) mySlot.currentItem = null;
                    Destroy(gameObject);
                }
            }
        }
    }
}