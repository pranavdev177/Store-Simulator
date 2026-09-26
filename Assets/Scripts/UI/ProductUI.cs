using TMPro;
using UnityEngine;

public class ProductUI : MonoBehaviour
{
    [SerializeField] TMP_Text nameText, priceText, amountInBoxText, boxPriceText, buttonText;
    [SerializeField] StockBoxController boxToSpawn;

    private float boxCost;
    private StockInfoSO product;

    public void Setup(StockInfoSO product)
    {
        this.product = product;

        nameText.text = product.objectName;
        priceText.text = "$" + product.price.ToString("F2");

        int boxAmount = boxToSpawn.GetStockAmount(product);

        amountInBoxText.text = boxAmount.ToString() + " per box";

        boxCost = boxAmount * product.price;

        boxPriceText.text = "$" + boxCost.ToString("F2");
        buttonText.text = "PAY: $" + boxCost.ToString("F2");
    }

    public void BuyBox()
    {
        StoreManager store = StoreManager.instance;

        if(!store.CheckMoneyAvailable(boxCost))
            return;

        store.SpendMoney(boxCost);

        StockBoxController box = Instantiate(boxToSpawn, store.stockSpawnPoint.position, Quaternion.identity);
        box.SetupBox(product);
    }

}
