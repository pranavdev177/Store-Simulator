using TMPro;
using UnityEngine;

public class FurnitureUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameText, priceText;
    private FurnitureInfoSO furniture;

    public void Setup(FurnitureInfoSO furniture)
    {
        this.furniture = furniture;

        nameText.text = furniture.objectName;
        priceText.text = "Price: " + furniture.price.ToString("F2");
    }

    public void BuyFurniture()
    {
        StoreManager store = StoreManager.instance;

        if(!store.CheckMoneyAvailable(furniture.price))
            return;

        store.SpendMoney(furniture.price);

        Instantiate(furniture.objectPrefab, store.furnitureSpawnPoint.position, Quaternion.identity);
    }
}
