using UnityEngine;

// Este es el Gestor de Recompensas (Singleton).
// Cuando el jugador completa una misión y va a cobrarla, este script se encarga de 
// leer qué premio le toca y dárselo en la mano (o tirárselo al suelo si no tiene hueco).
public class RewardController : MonoBehaviour
{
    public static RewardController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Lee la lista de premios de una misión y los va repartiendo uno a uno.
    public void GiveQuestReward(Quest quest)
    {
        // Si la misión no tiene premios, terminamos aquí
        if (quest?.questRewards == null) return;

        foreach (var reward in quest.questRewards)
        {
            // Comprobamos de qué tipo es el premio y actuamos en consecuencia
            switch (reward.type)
            {
                case RewardType.Item:
                    // Si es un objeto físico, usamos la función de abajo
                    GiveItemReward(reward.rewardID, reward.amount);
                    break;
                case RewardType.Gold:
                    // (Preparado para añadir un sistema de dinero en el futuro)
                    break;
                case RewardType.Experience:
                    // (Preparado para añadir un sistema de experiencia en el futuro)
                    break;
                case RewardType.Custom:
                    break;
            }
        }
    }

    // Lógica específica para entregar objetos (espadas, pociones, etc.)
    public void GiveItemReward(int itemID, int amount)
    {
        // Buscamos el objeto en el Catálogo Universal de nuestro juego
        var itemPrefab = FindAnyObjectByType<ItemDictionary>()?.GetItemPrefab(itemID);

        if (itemPrefab == null) return;

        // Entregamos la cantidad exacta que nos han pedido
        for (int i = 0; i < amount; i++)
        {
            // Intentamos guardarlo directamente en la mochila (AddItem).
            // Si AddItem devuelve 'false', significa que la mochila está llena.
            if (!InventoryController.Instance.AddItem(itemPrefab))
            {
                // Como no cabe, lo creamos físicamente en el mundo, tirado a los pies del jugador
                GameObject dropItem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);

                // Le damos un golpecito visual para que parezca que cae de verdad
                dropItem.GetComponent<BounceEffect>().StartBounce();
            }
            else
            {
                // Si sí cabía en la mochila, lanzamos la notificación de "Objeto Conseguido" en la pantalla
                itemPrefab.GetComponent<Item>().ShowPopUp();
            }
        }
    }
}