using UnityEngine;

public class HealthPotionItem : Item // ¡Heredamos de Item!
{
    [Header("Poción")]
    public int healAmount = 3;

    public override void UseItem()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();

            if (health != null && health.currentHealth < health.maxHealth)
            {
                health.Heal(healAmount);

                // SoundEffectManager.Play("DrinkPotion");

                // Le restamos 1 a la cantidad de este objeto
                RemoveFromStack(1);

                // Avisamos a tu InventoryController para que actualice la interfaz
                if (InventoryController.Instance != null)
                {
                    InventoryController.Instance.RebuildItemCounts();
                }

                if (quantity <= 0)
                {
                    Slot mySlot = GetComponentInParent<Slot>();
                    if (mySlot != null) mySlot.currentItem = null;
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.Log("Ya tienes la vida al máximo.");
            }
        }
    }
}