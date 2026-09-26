using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FurnitureInfoSO", menuName = "Objects/Furniture")]
public class FurnitureInfoSO : ScriptableObject
{
    public string objectName;
    public float price;

    [Header("Allowed Products")]
    public List<StockInfoSO.StockType> allowedStockTypes;

    public FurnitureController objectPrefab;
}
