using System.Collections.Generic;
using UnityEngine;

public class StockManager : MonoBehaviour
{
    [SerializeField] List<StockInfoSO> foodInfo, produceInfo;

    private List<StockInfoSO> allStock = new List<StockInfoSO>();
    private List<ShelfSpaceController> shelves = new List<ShelfSpaceController>();

    public static StockManager instance;

    void Awake()
    {
        allStock.AddRange(foodInfo);
        allStock.AddRange(produceInfo);

        for(int i = 0; i < allStock.Count; i++)
        {
            //if(allStock[i].currentPrice == 0)
                allStock[i].currentPrice = allStock[i].price;            
        }

        instance = this;
    }

    public void UpdatePrice(StockInfoSO stock, float newPrice)
    {
        for(int i = 0; i < allStock.Count; i++)
        {
            if(allStock[i] == stock)
            {
                allStock[i].currentPrice = newPrice;
            }
        }

        foreach(ShelfSpaceController shelf in shelves)
        {
            if(shelf.GetInfo() == stock)
                shelf.UpdateDisplayPrice(newPrice);
        }
    }

    public void RegisterShelf(ShelfSpaceController shelf)
    {
        shelves.Add(shelf);
    }

    public void ItemBought() => DayManager.instance.currentDayStats.itemsSold++;
}
