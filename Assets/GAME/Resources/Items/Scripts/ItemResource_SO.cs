using UnityEngine;

[CreateAssetMenu(fileName = "New ResourceItem", menuName = "GAME/ResourceItem")]
public class ResourceItem : ScriptableObject
{
    public string itemName;
    public string itemDescription;
    [Space]
    public int itemValue;
    [Space]
    public Sprite itemIcon;
}