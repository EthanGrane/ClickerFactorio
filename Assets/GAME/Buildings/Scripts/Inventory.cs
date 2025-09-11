/*
 * Hay que optimizar todos los datos usados apra reducir la carga, hay que usar Bytes para gurdar valores.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class Inventory
{
    public Slot[] _slots;
    
    public Inventory(int numberOfSlots, int slotSize)
    {
        _slots = new Slot[numberOfSlots];

        for (int i = 0; i < _slots.Length; i++)
            _slots[i] = new Slot(this, slotSize);
    }

    public void AddItemToInventory(Item item)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].IsItemAvaliableOnSlot(item))
            {
                _slots[i].AddItem(item);
                break;
            }
        }
    }

    public Item DequeueItemFromInventory()
    {
        Item removedItem = null;
        for (int i = 0; i < _slots.Length; i++)
        {
            removedItem = _slots[i].RemoveItem();
            if (removedItem != null)
                return removedItem;
        }    
        
        return removedItem;
    }

    public bool isInventoryFull()
    {
        bool isFull = true;
        
        for (int i = 0; i < _slots.Length; i++)
            if(_slots[i].IsSlotFull() == false)
                isFull = false;
        
        return isFull;
    }

    public bool isItemAvaliableOnInventory(Item item)
    {
        bool isAvaliable = false;
        
        for (int i = 0; i < _slots.Length; i++)
            if(_slots[i].IsItemAvaliableOnSlot(item))
                isAvaliable = true;
        
        return isAvaliable;
    }
}

[System.Serializable]
public class Slot
{
    public int _slotSize;
    public Queue<ItemInstance> _slotItems;
    // Debug
    public ItemInstance[] DEBUG_slotItems;
    public Item _itemFilter;
    [System.NonSerialized] private Inventory _inventoryReference;

    public Slot(Inventory slotInventoryReference ,int slotSize)
    {
        _slotSize = slotSize;
        _inventoryReference = slotInventoryReference;
        _slotItems = new Queue<ItemInstance>();
    }

    // Añade un item al slot siempre que el item sea igual al item preferente del inventario
    public void AddItem(Item item)
    {
        // Si el inventario esta vacio hay que Inicializar de nuevo el slot permitiendo colocar otgro tipo de item,
        // el codigo no soporta que el slot una vez vacio pueda tener otro item
        if (_itemFilter == null)
        {
            Debug.Log("Item Filter inicialized");
            _itemFilter = item;
        }        
        
        if (item == null)
        {
            Debug.LogWarning("Item is null");
            return;
        }

        if (item != _itemFilter)
        {
            Debug.LogWarning($"Item: {item.itemName} is not equal to slot item: {_itemFilter.itemName}");
            return;
        }
        
        _slotItems.Enqueue(new ItemInstance(item,1));
        
        DEBUG_slotItems = _slotItems.ToArray();
    }

    // Quita el ultimo item de la queue (Como todos los items dentro del slot son iguales con quitar el ultimo bastara.)
    public Item RemoveItem()
    {
        if (_slotItems == null)
        {
            Debug.LogWarning("Item is null");
            return null;
        }        
        if(_slotItems.Count == 0)
        {
            Debug.LogWarning("Item is empty");
            return null;
        }        
        
        DEBUG_slotItems = _slotItems.ToArray();
        _slotItems.Dequeue();
        return _itemFilter;
    }

    public int GetSlotSize()
    {
        return _slotSize;
    }

    public Item GetItem()
    {
        return _itemFilter;
    }

    public Inventory GetInventory()
    {
        return _inventoryReference;
    }

    // Comprueba si el filter es el mismo y comrpueba si hay espacio
    public bool IsItemAvaliableOnSlot(Item item)
    {
        if (item == null)
        {
            Debug.LogWarning("Item is null");
            return false;
        }

        // Inventario esta vacio y no esta inicializado
        if (_itemFilter == null)
        {
            return true;
        }
        
        if (item == _itemFilter)
        {
            if (_slotItems.Count < _slotSize)
            {
                return true;
            }        
        }
        
        return false;

    }

    public bool IsSlotFull()
    {
        return _slotItems.Count == _slotSize;
    }
}

[System.Serializable]
public class ItemInstance
{
    public Item baseItem;
    public int amount;

    public ItemInstance(Item item, int count = 1)
    {
        baseItem = item;
        amount = count;
    }
}

[CreateAssetMenu(fileName = "New Item", menuName = "GAME/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public string itemDescription;
    [Space]
    public int itemValue;
    [Space]
    public Sprite itemIcon;
}