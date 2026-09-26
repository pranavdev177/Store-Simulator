using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShoppingListGenerator : MonoBehaviour
{
    public static ShoppingListGenerator instance;

    // [SerializeField] CustomerShoppingProfileSO shoppingProfile;

    // public List<ShoppingListItem> shoppingList = new();

    void Awake()
    {
        instance = this;
    }

    // [ContextMenu("Generate List For Testing")]
    // public void GenerateListTest()
    // {
    //     shoppingList = new();
    //     int itemsToAdd = Random.Range(shoppingProfile.minItems, shoppingProfile.maxItems + 1);

    //     List<ProductWeight> availableProducts = new(shoppingProfile.possibleProducts);
    //     itemsToAdd = Mathf.Min(itemsToAdd, availableProducts.Count);

    //     for(int i = 0; i < itemsToAdd; i++)
    //     {
    //         ProductWeight chosen = GetWeightedRandomProduct(availableProducts);

    //         shoppingList.Add( new ShoppingListItem
    //         {
    //            product = chosen.product,
    //            amount = Random.Range(chosen.minAmount, chosen.maxAmount + 1)    
    //         });

    //         availableProducts.Remove(chosen); // prevents duplicates
    //     }
    // }

    public List<ShoppingListItem> GenerateList(CustomerShoppingProfileSO profile)
    {
        List<ShoppingListItem> list = new();
        int itemsToAdd = Random.Range(profile.minItems, profile.maxItems + 1);

        List<ProductWeight> availableProducts = new(profile.possibleProducts);
        itemsToAdd = Mathf.Min(itemsToAdd, availableProducts.Count);

        for(int i = 0; i < itemsToAdd; i++)
        {
            ProductWeight chosen = GetWeightedRandomProduct(availableProducts);

            list.Add( new ShoppingListItem
            {
               product = chosen.product,
               amount = Random.Range(chosen.minAmount, chosen.maxAmount + 1)    
            });

            availableProducts.Remove(chosen); // prevents duplicates
        }

        return list;
    }

    private ProductWeight GetWeightedRandomProduct(List<ProductWeight> products)
    {
        float totalWeight = 0;

        foreach (ProductWeight product in products)
            totalWeight += product.weight;

        float random = Random.Range(0f, totalWeight);

        foreach (ProductWeight product in products)
        {
            random -= product.weight;

            if (random <= 0)
                return product;
        }

        return products[^1];
    }
}
