using UnityEngine;
using TMPro; // Obligatorio para usar TextMeshPro

public class PlayerStatsUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI damageTextValue;

    // OnEnable se ejecuta automáticamente cada vez que esta página se hace visible
    private void OnEnable()
    {
        UpdateDamageUI();
    }

    public void UpdateDamageUI()
    {
        // 1. Buscamos al jugador en la escena
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 2. Sacamos su script de combate
            PlayerCombat combatScript = player.GetComponent<PlayerCombat>();
            if (combatScript != null)
            {
                // 3. Actualizamos el texto con su daño actual
                damageTextValue.text = combatScript.attackDamage.ToString();
            }
        }
    }
}