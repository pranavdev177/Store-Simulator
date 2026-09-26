using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomerShoppingProfileSO", menuName = "Customers/Customer Profile")]
public class CustomerShoppingProfileSO : ScriptableObject
{
    public string shoppingType;

    public int minItems;
    public int maxItems;

    public float guarunteedPriceTolerance;
    public float maxPriceTolerance;

    public float excellentCheckoutWaitTime;
    public float idealCheckoutWaitTime;
    public float acceptableCheckoutWaitTime;

    public float minWalkingSpeed;
    public float maxWalkingSpeed;
    
    public List<ProductWeight> possibleProducts;

    [Header("Review Importance")]
    public float priceImportance = 1f;
    public float stockImportance = 1f;
    public float serviceImportance = 1f;
}

[System.Serializable]
public class ProductWeight
{
    public StockInfoSO product;
    public float weight;

    public int minAmount;
    public int maxAmount;
}
