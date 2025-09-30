using UnityEngine;

public class MarketBuilding : MonoBehaviour, IInventory, IBuilding
{
    Inventory inventory;
    CellObject cellObject;

    public AudioClip sellSound;
    
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
            AudioManager.Instance.PlayOneShot3D(sellSound, transform.position).PitchVariation(0.25f).Volume(0.5f).MaxDistance(25).Play();
            Debug.Log($"{name}: Vendido '{soldItem}' por {soldItem.itemValue} (saldo actualizado).");
        }
        
    }

    public CellObject GetCellObject() => cellObject;
    public Inventory GetInventory() => inventory;
}
