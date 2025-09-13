using UnityEngine;

public class MarketBuilding : MonoBehaviour, IInventory, IBuilding
{
    Inventory inventory;
    CellObject cellObject;

    private void Awake()
    {
        // Fallback por si Initialize no se llama desde otro sistema.
        if (inventory == null)
            inventory = new Inventory(8, 64, "Market");
    }

    public void Initialize(CellObject cellObject)
    {
        this.cellObject = cellObject;
        if (inventory == null)
            inventory = new Inventory(8, 64, "Market");

        Debug.Log($"{name} initialized with inventory '{inventory.inventoryName}'");
    }

    public void PlanTick() { }

    public void ActionTick()
    {
        if (inventory == null)
        {
            Debug.LogWarning($"{name}: inventory es null. ¿Olvidaste llamar Initialize?");
            return;
        }

        // --- Opción A: Inventory con API tipo cola (Peek/Dequeue/isInventoryEmpty) ---
        if (inventory.isInventoryEmpty())
            return;

        // Vender TODO lo que haya en la cola (ajusta si quieres vender sólo 1 por tick).
        while (!inventory.isInventoryEmpty())
        {
            var peek = inventory.PeekItemFromInventory();
            if (peek == null) break; // seguridad

            var soldItem = inventory.DequeueItemFromInventory();
            if (soldItem == null)
            {
                Debug.LogWarning($"{name}: Dequeue devolvió null aunque Peek no lo era. Rompiendo loop.");
                break;
            }

            // Asegúrate de que soldItem.itemValue es el valor correcto (y que itemValue existe).
            GameManager.Instance?.AddMoney(soldItem.itemValue);
            Debug.Log($"{name}: Vendido '{soldItem}' por {soldItem.itemValue} (saldo actualizado).");
        }

        // --- Opción B: Si tu Inventory NO tiene cola y quieres operar por Slot ---
        // Descomenta y adapta según la API de Slot (por ejemplo el método para restar cantidad).
        /*
        Slot[] slots = inventory.GetSlots();
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            if (slot == null) continue;
            if (slot.GetSlotQuantity() <= 0) continue;
            if (slot.resourceItemFilter == null) continue;

            // IMPORTANTE: reemplaza RemoveFromSlot(1) por el método real de tu Slot para quitar items.
            slot.RemoveFromSlot(1); // <-- Ejemplo ficticio: adapta a tu API real.
            GameManager.Instance?.AddMoney(slot.resourceItemFilter.itemValue);
            Debug.Log($"{name}: Vendido 1 unidad de {slot.resourceItemFilter.name} por {slot.resourceItemFilter.itemValue}.");
        }
        */
    }

    public CellObject GetCellObject() => cellObject;
    public Inventory GetInventory() => inventory;
}
