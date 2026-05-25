using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

// Este script es el "Motor de Arrastre" (Drag & Drop) del juego.
// Se encarga de permitir al jugador coger objetos con el ratón, moverlos por la pantalla
// y decidir qué hacer al soltarlos: cambiarlos de hueco, apilarlos, intercambiarlos o tirarlos al suelo.
public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Referencias Internas")]
    private Transform originalParent;       // Memoria: ¿En qué hueco (Slot) estaba este objeto antes de cogerlo?
    private CanvasGroup canvasGroup;        // Controla la opacidad y las colisiones físicas de la interfaz
    private InventoryController inventoryController; // El mánager central del inventario

    [Header("Físicas de Caída al Suelo")]
    public float minDropDistance = 3.5f;
    public float maxDropDistance = 4.5f;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        inventoryController = InventoryController.Instance;
    }

    // 1. INICIO DEL ARRASTRE: Cuando hacemos clic izquierdo sobre el objeto y empezamos a mover el ratón
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Si pulsamos cualquier botón que no sea el izquierdo, ignoramos la acción
        if (eventData.button != PointerEventData.InputButton.Left) return;

        originalParent = transform.parent; // Guardamos el hueco original por si tenemos que devolverlo

        // Sacamos la imagen del objeto de su hueco y la ponemos en la capa más alta (root)
        // para que al moverlo no quede oculto por detrás de otras ventanas del menú.
        transform.SetParent(transform.root);

        // Hacemos que el objeto sea "transparente" para el ratón. 
        // Si no hacemos esto, el ratón chocaría con el propio objeto que estamos arrastrando 
        // y nunca podría detectar el hueco (Slot) que hay debajo al soltarlo.
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f; // Lo volvemos un poco traslúcido para dar sensación de "agarrado"
    }

    // 2. DURANTE EL ARRASTRE: Mientras movemos el ratón por la pantalla
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Hacemos que la posición de la imagen persiga exactamente a la posición del cursor (Nuevo Input System)
            transform.position = eventData.position;
        }
    }

    // 3. FIN DEL ARRASTRE: Cuando soltamos el clic izquierdo del ratón
    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        // Restauramos la opacidad y volvemos a hacer que el objeto sea sólido para el ratón
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        GameObject hitObject = eventData.pointerCurrentRaycast.gameObject; // ¿Qué hay justo debajo del ratón?
        Slot targetSlot = null;

        if (hitObject != null)
        {
            targetSlot = hitObject.GetComponent<Slot>(); // Comprobamos si soltamos directamente sobre un hueco vacío

            // Si no le dimos a un hueco vacío, sino a la imagen de otro objeto, 
            // buscamos el hueco (Slot) padre que contiene a ese otro objeto.
            if (targetSlot == null) targetSlot = hitObject.GetComponentInParent<Slot>();
        }

        if (targetSlot != null)
        {
            Slot oldSlot = originalParent.GetComponent<Slot>();

            // SITUACIÓN 1: El hueco de destino está completamente VACÍO
            if (targetSlot.currentItem == null)
            {
                if (oldSlot != null) oldSlot.currentItem = null; // Borramos nuestro rastro en el hueco viejo

                targetSlot.currentItem = gameObject; // Nos registramos en el hueco nuevo
                transform.SetParent(targetSlot.transform);
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // Nos centramos perfectamente

                inventoryController.RebuildItemCounts(); // Avisamos al mánager para que actualice las libretas
                return;
            }
            // SITUACIÓN 2: El hueco de destino YA TIENE UN OBJETO dentro
            else
            {
                Item existingItem = targetSlot.currentItem.GetComponent<Item>();
                Item myItem = GetComponent<Item>();

                // OPICIÓN 2.1: Son exactamente el mismo tipo de objeto -> Intentamos APILAR (Stack)
                if (existingItem != null && myItem != null && existingItem.ID == myItem.ID && existingItem.quantity < existingItem.maxStackSize)
                {
                    // Calculamos cuántos caben en el montón de destino sin pasarnos del límite máximo
                    int amountToMove = Mathf.Min(myItem.quantity, existingItem.maxStackSize - existingItem.quantity);
                    existingItem.AddToStack(amountToMove); // Se los sumamos al de destino
                    myItem.RemoveFromStack(amountToMove);  // Nos los restamos a nosotros mismos

                    // Si tras traspasar los objetos nos hemos quedado a cero, destruimos nuestro clon visual
                    if (myItem.quantity <= 0)
                    {
                        if (oldSlot != null) oldSlot.currentItem = null;
                        Destroy(gameObject);
                        inventoryController.RebuildItemCounts();
                        return;
                    }
                }
                // OPCIÓN 2.2: Son objetos diferentes o el montón está lleno -> Los INTERCAMBIAMOS de sitio (Swap)
                else if (oldSlot != null)
                {
                    GameObject otherItem = targetSlot.currentItem;

                    // 1. Mandamos el objeto que ya estaba allí a nuestro hueco original
                    otherItem.transform.SetParent(oldSlot.transform);
                    otherItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    oldSlot.currentItem = otherItem;

                    // 2. Nos colocamos nosotros en este nuevo hueco
                    targetSlot.currentItem = gameObject;
                    transform.SetParent(targetSlot.transform);
                    GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    inventoryController.RebuildItemCounts();
                    return;
                }
            }
        }

        HotbarController hotbarController = FindObjectOfType<HotbarController>();

        // Comprobamos matemáticamente si el ratón sigue dentro del rectángulo de los menús
        bool insideInventory = RectTransformUtility.RectangleContainsScreenPoint(inventoryController.inventoryPanel.GetComponent<RectTransform>(), eventData.position);
        bool insideHotbar = false;

        if (hotbarController != null && hotbarController.hotbarPanel != null)
        {
            insideHotbar = RectTransformUtility.RectangleContainsScreenPoint(hotbarController.hotbarPanel.GetComponent<RectTransform>(), eventData.position);
        }

        // Si el ratón NO está en el inventario Y TAMPOCO en la barra rápida... ¡Lo tiramos al suelo!
        if (!insideInventory && !insideHotbar)
        {
            Item itemComponent = GetComponent<Item>();
            if (itemComponent != null && itemComponent.itemPrefab != null)
            {
                Slot oldSlot = originalParent.GetComponent<Slot>();
                if (oldSlot != null) oldSlot.currentItem = null; // Vaciamos nuestro hueco en el código

                // 1. Buscamos al jugador
                Transform player = GameObject.FindGameObjectWithTag("Player").transform;

                // 2. Calculamos dónde está el ratón en el mundo 2D
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(eventData.position);
                mouseWorldPos.z = player.position.z; // Igualamos la profundidad (Z) para evitar fallos de colisión

                // 3. Sacamos la dirección exacta (la flecha imaginaria) desde el jugador hacia el ratón
                Vector3 dropDirection = (mouseWorldPos - player.position).normalized;

                // Disparamos un rayo invisible desde el jugador hasta la distancia máxima de caída
                float distance = Random.Range(minDropDistance, maxDropDistance);

                RaycastHit2D[] hits = Physics2D.RaycastAll(player.position, dropDirection, distance);

                foreach (RaycastHit2D hit in hits)
                {
                    // Si el rayo choca contra algo sólido que NO es una zona invisible (Trigger) 
                    // y que NO es el propio jugador... ¡Significa que es una pared!
                    if (hit.collider != null && !hit.collider.isTrigger && !hit.collider.CompareTag("Player"))
                    {
                        // Acortamos la distancia de caída para que choque y se quede justo delante de la pared
                        distance = hit.distance - 0.2f; // El 0.2f es un pequeño margen para que no se incruste

                        // Si estábamos totalmente pegados a la pared, evitamos que la distancia se vuelva negativa
                        if (distance < 0.2f) distance = 0.2f;

                        break; // Como ya hemos chocado con el primer muro, dejamos de buscar
                    }
                }

                // 4. El objeto caerá a 1.5 unidades de distancia del jugador
                Vector3 dropPosition = player.position + (dropDirection * distance);

                // Creamos físicamente en el mapa tantos objetos como hubiera en el montón
                for (int i = 0; i < itemComponent.quantity; i++)
                {
                    // Pequeña dispersión para que no caigan 10 pociones fusionadas en el mismo píxel
                    Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0f);

                    GameObject droppedItem = Instantiate(itemComponent.itemPrefab, dropPosition + randomOffset, Quaternion.identity);

                    Item droppedItemComp = droppedItem.GetComponent<Item>();
                    if (droppedItemComp != null)
                    {
                        droppedItemComp.isPickedUp = false; // Garantizamos que se pueda volver a coger
                    }

                    // Si el objeto físico tiene nuestro script de rebote, lo hacemos saltar visualmente
                    BounceEffect bounce = droppedItem.GetComponent<BounceEffect>();
                    if (bounce != null) bounce.StartBounce();
                }

                Destroy(gameObject); // Borramos la imagen 2D de la pantalla
                inventoryController.RebuildItemCounts();
                return;
            }
        }

        // Si soltamos el objeto en un borde inválido (ej: el título del menú) y no hizo ninguna 
        // de las acciones anteriores, lo devolvemos a su casilla original como si nada hubiera pasado.
        if (transform.parent == transform.root)
        {
            transform.SetParent(originalParent);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }

    // 4. CLIC SECUNDARIO: Dividir montones por la mitad
    public void OnPointerClick(PointerEventData eventData)
    {
        // Si hacemos clic derecho sobre un objeto de la mochila...
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            SplitStack();
        }
    }

    // Función que coge un montón (ej: 10 pociones) y lo divide en dos (5 y 5)
    private void SplitStack()
    {
        Item item = GetComponent<Item>();
        if (item == null || item.quantity <= 1) return; // Si solo hay 1, no se puede dividir

        int splitAmount = item.quantity / 2; // Calculamos la mitad exacta
        if (splitAmount <= 0) return;

        item.RemoveFromStack(splitAmount); // Le quitamos esa mitad a nuestro montón original

        // Clonamos la imagen del objeto y le asignamos la nueva cantidad
        GameObject newItem = Instantiate(item.gameObject);
        newItem.GetComponent<Item>().quantity = splitAmount;
        newItem.GetComponent<Item>().UpdateQuantityDisplay();

        if (inventoryController == null || newItem == null) return;

        // Buscamos a la desesperada el primer hueco que esté libre en la mochila
        foreach (Transform slotTransform in inventoryController.inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                // Metemos la nueva mitad en este hueco vacío
                slot.currentItem = newItem;
                newItem.transform.SetParent(slot.transform);

               
                // SEGURO DE ESCALA (Previene el bug visual de los sprites gigantes)
                RectTransform rect = newItem.GetComponent<RectTransform>();
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;

                inventoryController.RebuildItemCounts(); // Actualizamos la memoria global
                return;
            }
        }

        // Si el bucle termina y no había espacio libre en la mochila, cancelamos todo el proceso:
        // Le devolvemos la cantidad prestada a nuestro objeto original y borramos el clon.
        item.AddToStack(splitAmount);
        Destroy(newItem);
    }
}