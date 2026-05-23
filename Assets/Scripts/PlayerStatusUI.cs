using UnityEngine;
using TMPro; // Obligatorio para usar los textos en alta definición (TextMeshPro)

// Este script se encarga de actualizar los números de la pantalla del menú
// para que el jugador pueda ver sus estadísticas (como el daño que hace).
public class PlayerStatsUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI damageTextValue; // El texto donde escribiremos el número de daño

    // OnEnable es una función mágica de Unity que se dispara automáticamente 
    // CADA VEZ que esta ventana se enciende (se hace visible en pantalla).
    private void OnEnable()
    {
        // Así nos aseguramos de que los números estén siempre actualizados al abrir el menú
        UpdateDamageUI();
    }

    public void UpdateDamageUI()
    {
        // 1. Buscamos al jugador en la escena
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 2. Le pedimos su libreta de combate (donde guarda sus estadísticas)
            PlayerCombat combatScript = player.GetComponent<PlayerCombat>();
            if (combatScript != null)
            {
                // 3. Escribimos su daño actual en la pantalla
                damageTextValue.text = combatScript.attackDamage.ToString();
            }
        }
    }
}