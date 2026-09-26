using UnityEngine;

[CreateAssetMenu(fileName = "StockInfoSO", menuName = "Objects/Stock Object")]
public class StockInfoSO : ScriptableObject
{
    public enum StockType
    {
        cereal, bigDrink, chipsTube, fruit, fruitLarge
    }

    public string objectName;
    public StockType stockType;
    public float price, currentPrice;

    public int maxPickupAmount;

    public StockObject objectPrefab;
}
