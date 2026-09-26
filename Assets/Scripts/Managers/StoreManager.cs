using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    public static StoreManager instance;

    public Transform stockSpawnPoint;
    public Transform furnitureSpawnPoint;

    public float currentMoney = 1000f;
    public List<FurnitureController> shelvingCases = new List<FurnitureController>();
    public List<Checkout> checkouts = new List<Checkout>();

    public bool HasShelf() => shelvingCases.Count > 0;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        UIController.instance.UpdateMoney(currentMoney);
    }

    public void ItemBought(PurchasedItem item)
    {
        DayManager.instance.currentDayStats.revenue += item.purchasePrice;
        DayManager.instance.currentDayStats.profit += item.purchasePrice - item.stock.stockInfo.price;
    }

    public void CompleteSale(float amount) 
    {
        currentMoney += amount;
        UIController.instance.UpdateMoney(currentMoney);
    }

    public void SpendMoney(float amount)
    {
        if(currentMoney > amount)
            currentMoney -= amount;

        UIController.instance.UpdateMoney(currentMoney);
    }

    public bool CheckMoneyAvailable(float amountToCheck) =>  currentMoney > amountToCheck;

    public void RegisterShelvingCase(FurnitureController shelfCase) => shelvingCases.Add(shelfCase);

    public FurnitureController GetRandomShelf() => shelvingCases[Random.Range(0, shelvingCases.Count)];

    public FurnitureController FindShelfWithProduct(StockInfoSO product)
    {
        foreach(FurnitureController shelfCase in shelvingCases)
        {
            if(shelfCase.HasProduct(product))
                return shelfCase;
        }

        return null;
    }

    public void RegisterCheckout(Checkout checkout) => checkouts.Add(checkout);

    public Checkout GetCheckout() => checkouts[0];
}
