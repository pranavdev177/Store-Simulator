using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShelfSpaceController : MonoBehaviour
{
    private StockInfoSO info;
    private List<StockObject> objectsOnShelf = new();

    [SerializeField] List<Transform> bigDrinkPoints, cerealPoints, tubeChipPoints, fruitPoints, largeFruitPoints;
    [SerializeField] TMP_Text shelfLabel;

    private FurnitureController furniture;

    void Awake()
    {
        furniture = GetComponentInParent<FurnitureController>();
    }

    void Start()
    {
        StockManager.instance.RegisterShelf(this);
    }

    public bool PlaceStock(StockObject objectToPlace)
    {
        List<Transform> pointsToUse = GetPlacementPoints(objectToPlace.stockInfo);

        if(!furniture.CanHold(objectToPlace.stockInfo))
            return false;

        if(objectsOnShelf.Count == 0)
            info = objectToPlace.stockInfo;
        else
        {
            if(objectToPlace.stockInfo != info || objectsOnShelf.Count >= pointsToUse.Count)
                return false;  
        }

        objectToPlace.MakePlaced();
        objectToPlace.transform.SetParent(pointsToUse[objectsOnShelf.Count]);
        objectsOnShelf.Add(objectToPlace);
        UpdateDisplayPrice(info.currentPrice);


        if(AudioManager.instance != null)
            AudioManager.instance.PlaySFX(SoundEffectType.ItemPlace);

        return true;
    }

    public StockObject GetStock()
    {
        if(objectsOnShelf.Count == 0)
            return null;

        StockObject lastObject = objectsOnShelf[^1];
        objectsOnShelf.RemoveAt(objectsOnShelf.Count - 1);

        if(objectsOnShelf.Count == 0)
        {
            shelfLabel.text = "";
            info = null;
        }

        return lastObject;
    }

    private List<Transform> GetPlacementPoints(StockInfoSO info)
    {
        switch(info.stockType){
            case StockInfoSO.StockType.bigDrink: return bigDrinkPoints;
            case StockInfoSO.StockType.cereal: return cerealPoints;
            case StockInfoSO.StockType.chipsTube: return tubeChipPoints;
            case StockInfoSO.StockType.fruit: return fruitPoints;
            case StockInfoSO.StockType.fruitLarge: return largeFruitPoints;
        }

        Debug.LogError($"Unhandled stock type: {info.stockType}");
        return null;    
    }

    public void StartPriceUpdate()
    {
        if(objectsOnShelf.Count > 0)
        {
            UIController.instance.OpenUpdatePrice(info);
        }
    }

    public void UpdateDisplayPrice(float price)
    {
        if(objectsOnShelf.Count == 0)
            return;

        info.currentPrice = price;

        shelfLabel.text = "$" + info.currentPrice.ToString("F2");
    }

    public List<StockObject> TakeStock(int amount)
    {
        amount = Mathf.Min(amount, objectsOnShelf.Count);

        List<StockObject> stockTaken = new();

        for (int i = 0; i < amount; i++)
        {
            StockObject stock = GetStock();

            if(stock == null)
                break;

            stockTaken.Add(stock);
        }

        return stockTaken;
    }

    public StockInfoSO GetInfo() => info;
}
