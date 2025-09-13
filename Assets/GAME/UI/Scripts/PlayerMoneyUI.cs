using UnityEngine;
using TMPro;
    
public class PlayerMoneyUI : MonoBehaviour
{
    public TextMeshProUGUI playerMoneyText;
    
    public void Start()
    {
        GameManager.Instance.onPlayerMoneyChanged += (int amount) =>
        {
            playerMoneyText.text = amount.ToString();
        };
    }
}
