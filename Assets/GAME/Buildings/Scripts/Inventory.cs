/*
 * Hay que optimizar todos los datos usados apra reducir la carga, hay que usar Bytes para gurdar valores.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class Inventory
{
    public string inventoryName;
    public Slot[] _slots;

    private int numberOfSlots;
    private int slotSize;
    public event Action OnItemAdded;
    public event Action OnItemRemoved;
    
    public Inventory(int numberOfSlots, int slotSize, string inventoryName)
    {
        this.inventoryName = inventoryName;
        this.slotSize = slotSize;
        this.numberOfSlots = numberOfSlots;
        
        _slots = new Slot[numberOfSlots];

        for (int i = 0; i < _slots.Length; i++)
            _slots[i] = new Slot(this, slotSize);
    }

    public void AddItemToInventory(ResourceItem resourceItem)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].IsItemAvaliableOnSlot(resourceItem))
            {
                _slots[i].AddItem(resourceItem);
                OnItemAdded?.Invoke();
                break;
            }
        }
    }

    public ResourceItem DequeueItemFromInventory()
    {
        ResourceItem removedResourceItem = null;
        for (int i = 0; i < _slots.Length; i++)
        {
            removedResourceItem = _slots[i].RemoveItem();
            if (removedResourceItem != null)
            {
                OnItemRemoved?.Invoke();
                return removedResourceItem;
            }        
        }    
        
        return removedResourceItem;
    }

    public void ClearInventory()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i] = new Slot(this, slotSize);
        }
    }

    public ResourceItem PeekItemFromInventory()
    {
        ResourceItem peekedResourceItem = null;

        if (_slots.Length > 0)
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].PeekItem() != null)
                {
                    peekedResourceItem = _slots[i].resourceItemFilter;
                    if (peekedResourceItem != null)
                        return peekedResourceItem;
                }
            }
        }

        Debug.LogWarning("No resourceItem found");
        return peekedResourceItem;
    }
    
    public Slot[] GetSlots() => _slots;

    public bool isInventoryFull()
    {
        bool isFull = true;
        
        for (int i = 0; i < _slots.Length; i++)
            if(_slots[i].IsSlotFull() == false)
                isFull = false;
        
        return isFull;
    }

    public bool isInventoryEmpty()
    {
        bool isEmpty = true;

        for (int i = 0; i < _slots.Length; i++)
            if (_slots[i].IsSlotEmpty() == false)
                isEmpty = false;
        
        return isEmpty;
    }

    public bool isItemAvaliableOnInventory(ResourceItem resourceItem)
    {
        bool isAvaliable = false;
        
        for (int i = 0; i < _slots.Length; i++)
            if(_slots[i].IsItemAvaliableOnSlot(resourceItem))
                isAvaliable = true;
        
        return isAvaliable;
    }
}

[System.Serializable]
public class Slot
{
    public int _slotSize;
    public Queue<ItemInstance> _slotItems;
    public int numberOfItems;
    public ResourceItem resourceItemFilter;
    [System.NonSerialized] private Inventory _inventoryReference;

    public Slot(Inventory slotInventoryReference ,int slotSize)
    {
        _slotSize = slotSize;
        _inventoryReference = slotInventoryReference;
        _slotItems = new Queue<ItemInstance>();
        resourceItemFilter = null;
    }

    // Añade un resourceItem al slot siempre que el resourceItem sea igual al resourceItem preferente del inventario
    public void AddItem(ResourceItem resourceItem)
    {
        // Si el inventario esta vacio hay que Inicializar de nuevo el slot permitiendo colocar otgro tipo de resourceItem,
        // el codigo no soporta que el slot una vez vacio pueda tener otro resourceItem
        if (resourceItemFilter == null)
        {
            Debug.Log("ResourceItem Filter inicialized");
            resourceItemFilter = resourceItem;
        }        
        
        if (resourceItem == null)
        {
            Debug.LogWarning("ResourceItem is null");
            return;
        }

        if (resourceItem != resourceItemFilter)
        {
            Debug.LogWarning($"ResourceItem: {resourceItem.itemName} is not equal to slot resourceItem: {resourceItemFilter.itemName}");
            return;
        }
        
        _slotItems.Enqueue(new ItemInstance(resourceItem,1));
        numberOfItems = _slotItems.Count;
    }
    
    // Quita el ultimo resourceItem de la queue (Como todos los items dentro del slot son iguales con quitar el ultimo bastara.)
    public ResourceItem RemoveItem()
    {

        if (_slotItems == null)
        {
            Debug.LogWarning("ResourceItem is null");
            return null;
        }        
        if(_slotItems.Count == 0)
        {
            Debug.LogWarning("ResourceItem is empty");
            return null;
        }        
        
        _slotItems.Dequeue();
        numberOfItems = _slotItems.Count;
        
        return resourceItemFilter;
    }

    public ItemInstance PeekItem()
    {
        if(_slotItems == null)
            return null;
        if(_slotItems.Count == 0)
            return null;
        
        return _slotItems.Peek();
    }

    public int GetSlotQuantity()
    {
        return _slotItems.Count;
    }

    public bool IsSlotEmpty()
    {
        return _slotItems.Count == 0;
    }
    
    public int GetSlotSize()
    {
        return _slotSize;
    }

    public ResourceItem GetItem()
    {
        return resourceItemFilter;
    }

    public Inventory GetInventory()
    {
        return _inventoryReference;
    }

    // Comprueba si el filter es el mismo y comrpueba si hay espacio
    public bool IsItemAvaliableOnSlot(ResourceItem resourceItem)
    {
        if (resourceItem == null)
        {
            /*
            Debug.LogWarning("ResourceItem is null");
            */
            return false;
        }

        // Inventario esta vacio y no esta inicializado
        if (resourceItemFilter == null)
        {
            return true;
        }
        
        if (resourceItem == resourceItemFilter)
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
    [FormerlySerializedAs("baseItem")] public ResourceItem baseResourceItem;
    public int amount;

    public ItemInstance(ResourceItem resourceItem, int count = 1)
    {
        baseResourceItem = resourceItem;
        amount = count;
    }
}