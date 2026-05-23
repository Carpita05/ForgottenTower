using UnityEngine;
using UnityEngine.EventSystems;

// Este script es el responsable de que puedas hacer clic, arrastrar y soltar objetos 
// dentro de la mochila. Controla qué pasa si mezclas dos objetos iguales (se apilan), 
// si los cambias de sitio (se intercambian) o si los tiras fuera de la ventana (caen al suelo).
public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private InventoryController inventoryController;

    [Header("Físicas de Caída")]
    public float minDropDistance = 2f; // Distancia mínima a la que cae el objeto al tirarlo
    public float maxDropDistance = 2f; // Distancia máxima a la que cae

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        inventoryController = InventoryController.Instance;
    }

    // 1. Cuando hacemos CLIC y empezamos a arrastrar el objeto...
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent; // Memorizamos en qué hueco estaba

        // Lo sacamos de su hueco y lo ponemos por encima de toda la interfaz para que no quede oculto tras otras ventanas
        transform.SetParent(transform.root);

        canvasGroup.blocksRaycasts = false; // Desactivamos su colisión para que el ratón pueda "ver" los huecos que hay debajo
        canvasGroup.alpha = 0.6f;           // Lo hacemos semitransparente para ver dónde lo estamos soltando
    }

    // 2. Mientras movemos el ratón...
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position; // El dibujo del objeto persigue al cursor
    }

    // 3. Cuando SOLTAMOS el clic del ratón...
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f; // Vuelve a ser totalmente opaco (sólido)

        // Miramos qué hay exactamente debajo del ratón al soltar
        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>();

        // Si soltamos justo encima del dibujo de otro objeto, buscamos a qué hueco pertenece ese objeto
        if (dropSlot == null)
        {
            GameObject item = eventData.pointerEnter;
            if (item != null && item.transform.parent != null)
            {
                dropSlot = item.transform.parent.GetComponent<Slot>();
            }
        }

        Slot originalSlot = originalParent.GetComponent<Slot>();

        // CASO A: Soltamos el objeto dentro de un hueco válido del inventario
        if (dropSlot != null)
        {
            // ¿El hueco ya estaba ocupado por otro objeto?
            if (dropSlot.currentItem != null)
            {
                Item draggedItem = GetComponent<Item>();
                Item targetItem = dropSlot.currentItem.GetComponent<Item>();

                // Si los dos objetos son idénticos (ej: dos pociones), los sumamos en un solo montón
                if (draggedItem.ID == targetItem.ID)
                {
                    targetItem.AddToStack(draggedItem.quantity);
                    originalSlot.currentItem = null;
                    Destroy(gameObject);
                }
                // Si son objetos diferentes (ej: espada y escudo), intercambiamos sus posiciones
                else
                {
                    // Movemos el objeto viejo al hueco original
                    dropSlot.currentItem.transform.SetParent(originalSlot.transform);
                    originalSlot.currentItem = dropSlot.currentItem;
                    dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    // Ponemos el objeto que arrastrábamos en el nuevo hueco
                    transform.SetParent(dropSlot.transform);
                    dropSlot.currentItem = gameObject;
                    GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                }
            }
            // ¿El hueco estaba vacío? Pues simplemente lo dejamos ahí
            else
            {
                originalSlot.currentItem = null;
                transform.SetParent(dropSlot.transform);
                dropSlot.currentItem = gameObject;
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }
        // CASO B: Soltamos el objeto fuera de un hueco válido
        else
        {
            // Si el ratón está fuera de la ventana del inventario, tiramos el objeto al suelo
            if (!IsWithinInventory(eventData.position))
            {
                DropItem(originalSlot);
            }
            // Si estábamos dentro de la ventana pero no apuntamos bien a un hueco, devolvemos el objeto a su sitio original
            else
            {
                transform.SetParent(originalParent);
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }
    }

    // Comprueba si el ratón está apuntando encima del panel del inventario
    bool IsWithinInventory(Vector2 mousePosition)
    {
        RectTransform inventoryRec = originalParent.parent.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(inventoryRec, mousePosition);
    }

    // Tira un objeto físico al mundo del juego (al suelo)
    void DropItem(Slot originalSlot)
    {
        Item item = GetComponent<Item>();
        int quantity = item.quantity;

        // Si tenemos un montón (ej: 5 pociones), solo tiramos 1 y el resto se queda en la mochila
        if (quantity > 1)
        {
            item.RemoveFromStack();
            transform.SetParent(originalParent);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            quantity = 1;
        }
        else
        {
            originalSlot.currentItem = null;
        }

        // Buscamos dónde está el jugador para que el objeto caiga a sus pies
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (playerTransform == null) return;

        // Calculamos una posición aleatoria muy cerquita del jugador
        Vector2 dropOffset = Random.insideUnitCircle * Random.Range(minDropDistance, maxDropDistance);
        Vector2 dropPosition = (Vector2)playerTransform.position + dropOffset;

        // Creamos el objeto en el mundo real 3D/2D
        GameObject dropItem = Instantiate(gameObject, dropPosition, Quaternion.identity);
        Item droppedItem = dropItem.GetComponent<Item>();
        droppedItem.quantity = 1;

        // Le damos un golpecito visual para que salte al caer (usando nuestro BounceEffect)
        dropItem.GetComponent<BounceEffect>().StartBounce();

        // Si tiramos el último objeto del montón, destruimos la imagen de la mochila
        if (quantity <= 1 && originalSlot.currentItem == null)
        {
            Destroy(gameObject);
        }

        // Avisamos a la mochila de que haga recuento total de cosas
        InventoryController.Instance.RebuildItemCounts();
    }

    // Detecta clics normales (sin arrastrar)
    public void OnPointerClick(PointerEventData eventData)
    {
        // Si hacemos Clic Derecho sobre un montón, lo partimos por la mitad
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            SplitStack();
        }
    }

    // Parte un montón de objetos (ej: 10 flechas) en dos mitades (5 y 5)
    private void SplitStack()
    {
        Item item = GetComponent<Item>();
        if (item == null || item.quantity <= 1) return;

        int splitAmount = item.quantity / 2;
        if (splitAmount <= 0) return;

        item.RemoveFromStack(splitAmount); // Le quitamos la mitad al montón original

        GameObject newItem = item.CloneItem(splitAmount); // Creamos un nuevo clon con esa mitad

        if (inventoryController == null || newItem == null) return;

        // Buscamos el primer hueco vacío de la mochila para meter la nueva mitad
        foreach (Transform slotTransform in inventoryController.inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                slot.currentItem = newItem;
                newItem.transform.SetParent(slot.transform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                return;
            }
        }

        // Si no había espacio libre en la mochila, cancelamos la operación y le devolvemos los objetos al montón original
        item.AddToStack(splitAmount);
        Destroy(newItem);
    }
}