using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{

    Transform originalParent;
    CanvasGroup canvasGroup;

    public float minDropDistance = 2f; //Distancia mínima para dropear el item
    public float maxDropDistance = 2f; //Distancia máxima para dropear el item

    private InventoryController inventoryController;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        inventoryController = InventoryController.Instance;
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent; //Guardar el padre original
        transform.SetParent(transform.root); //El canva encima de otros canvas
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;  //Transparencia al arrastrar
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position; //Sigue el raton
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f; //Vuele opaco

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>(); //Slot donde se deja el item

        if(dropSlot == null)
        {
            GameObject item = eventData.pointerEnter;
            if (item != null)
            {
                dropSlot = item.transform.parent.GetComponent<Slot>();
            }
        }


        Slot originalSlot = originalParent.GetComponent<Slot>(); //Slot original del item

        if(dropSlot != null)
        {
            //Si el slot donde se deja el item tiene otro item, intercambiarlos
            if (dropSlot.currentItem != null)
            {
                Item draggedItem = GetComponent<Item>();
                Item targetItem = dropSlot.currentItem.GetComponent<Item>();

                if (draggedItem.ID == targetItem.ID)
                {
                    targetItem.AddToStack(draggedItem.quantity);
                    originalSlot.currentItem = null; //El slot original queda vacio
                    Destroy(gameObject); //Destruir el item arrastrado
                }
                else
                {
                    dropSlot.currentItem.transform.SetParent(originalSlot.transform);
                    originalSlot.currentItem = dropSlot.currentItem;
                    dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    transform.SetParent(dropSlot.transform);
                    dropSlot.currentItem = gameObject;
                    GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //Centrar en el slot

                }

            }
            else
            {
                originalSlot.currentItem = null; //El slot original queda vacio
                transform.SetParent(dropSlot.transform);
                dropSlot.currentItem = gameObject;
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //Centrar en el slot

            }
        }
        else
        {
            //Si donde estamos dropeando no es está en el inventario
            //Dropea el item
            if (!IswithinInventory(eventData.position))
            {
                //Drop el item
                DropItem(originalSlot);

            }
            else
            {
                //Si esta
                //Volver al slot original
                transform.SetParent(originalParent);
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //Centrar en el 
            }
        }
    }
    bool IswithinInventory(Vector2 mousePosition)
    {
        RectTransform inventoryRec =  originalParent.parent.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(inventoryRec, mousePosition);
    }
    void DropItem(Slot originalSlot) {

        Item item = GetComponent<Item>();
        int quantity = item.quantity; //Cantidad a dropear

        if (quantity > 1)
        {
            item.RemoveFromStack();

            transform.SetParent(originalParent);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero; //Centrar en el slot
            quantity = 1; //Solo dropeamos 1 item del stack
        }
        else
        {
            originalSlot.currentItem = null;
        }


        //Find player
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        if (playerTransform == null)
        {
            Debug.LogError("Player not found in the scene.");
            return;
        }
        //Posición de drop random entre los rangos establecidos
        Vector2 dropOffset = Random.insideUnitCircle * Random.Range(minDropDistance, maxDropDistance);
        Vector2 dropPosition = (Vector2)playerTransform.position + dropOffset;

        //Instanciar el item droppeado en el mundo y rebotarlo
        GameObject dropItem = Instantiate(gameObject, dropPosition, Quaternion.identity);
        Item droppedItem = dropItem.GetComponent<Item>();
        droppedItem.quantity = 1;

        dropItem.GetComponent<BounceEffect>().StartBounce();

        //Destruir el item del inventario UI
        if (quantity <=1 && originalSlot.currentItem == null)
        {
            Destroy(gameObject);
        }

        InventoryController.Instance.RebuildItemCounts(); //Actualizar el conteo de items en el inventario

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            SplitStack();
        }
    }

    private void SplitStack()
    {
        Item item = GetComponent<Item>();
        if (item == null || item.quantity <= 1) return;

        int splitAmount = item.quantity / 2; //Cantidad a separar
        if(splitAmount <= 0) return;

        item.RemoveFromStack(splitAmount); //Reducir el stack original

        GameObject newItem = item.CloneItem(splitAmount); //Crear un nuevo item con la cantidad separada

        if (inventoryController == null || newItem == null) return;

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
        item.AddToStack(splitAmount); //Si no hay espacio, devolver la cantidad al stack original
        Destroy(newItem); //Destruir el item clonado que no se pudo colocar
    }
}
