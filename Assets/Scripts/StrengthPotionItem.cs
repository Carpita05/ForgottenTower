using UnityEngine;

public class StrengthPotionItem : Item // ¡Heredamos de Item igual que la de vida!
{
    [Header("Poción de Fuerza")]
    public int damageIncreaseAmount = 1;

    public override void UseItem()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerCombat combatScript = player.GetComponent<PlayerCombat>();

            if (combatScript != null)
            {
                // 1. Subimos el daño de forma permanente
                combatScript.IncreaseDamage(damageIncreaseAmount);

                // Opcional: Reproducir sonido de beber
                // SoundEffectManager.Play("DrinkPotion");

                // 2. Le restamos 1 a la cantidad de este objeto en el inventario
                RemoveFromStack(1);

                // 3. Avisamos al InventoryController para que actualice la interfaz visual
                if (InventoryController.Instance != null)
                {
                    InventoryController.Instance.RebuildItemCounts();
                }

                // 4. Si la cantidad llega a 0, vaciamos el slot y destruimos el objeto
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