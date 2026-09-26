using System.Collections.Generic;
using TMPro;
using UnityEngine;

enum CheckoutState
{
    Idle,
    Scanning,
    WaitingForPayment
}

public class Checkout : MonoBehaviour
{
    [SerializeField] Transform cashierPoint;

    [Header("UI")]
    [SerializeField] TMP_Text priceText;
    [SerializeField] TMP_Text currentItemText;
    [SerializeField] GameObject checkoutScreen;

    [Header("Queue")]
    [SerializeField] Transform queuePoint;
    [SerializeField] float spaceBetweenCustomers;

    [Header("Scanning")]
    [SerializeField] float timeToScan;
    [SerializeField] Transform scanPoint;

    private List<Customer> customersInQueue = new List<Customer>();

    private CheckoutState state = CheckoutState.Idle;
    private Customer currentCustomer;
    private IReadOnlyList<PurchasedItem> currentItems;
    
    private StockObject currentItem;
    private int currentItemIndex;
    private float runningTotal;
    private float timer;

    private Cashier cashier;
    private bool HasCashier => cashier != null;

    void Start()
    {
        StoreManager.instance.RegisterCheckout(this);
        HidePrice();
    }

    void Update()
    {
        if(timer > 0)
            timer -= Time.deltaTime;

        switch (state)
        {
            case CheckoutState.Idle:
                if(HasCashier && timer <= 0)
                    TryStartCheckout();

                break;

            case CheckoutState.Scanning:
                HandleScanning();
                break;

            case CheckoutState.WaitingForPayment:
                if(timer <= 0 && HasCashier)
                    CheckoutCustomer();

                break;
        }
    }

    public void PlayerPressedCheckout()
    {
        if(cashier)
            return;

        if(CanCheckout() && state == CheckoutState.Idle)
            TryStartCheckout();

        if(state == CheckoutState.WaitingForPayment)
            CheckoutCustomer();
    }

    private void HidePrice()
    {
        checkoutScreen.SetActive(false);
    }


    private void TryStartCheckout()
    {
        if(!CanCheckout())
            return;

        currentCustomer = customersInQueue[0];
        currentItems = currentCustomer.GetItemsInBag();

        currentItemIndex = 0;
        runningTotal = 0;
        timer = timeToScan;

        checkoutScreen.SetActive(true);
        currentItemText.text = "Scanning...";
        priceText.text = "$0.00";

        state = CheckoutState.Scanning;

        currentItem = Instantiate(currentItems[currentItemIndex].stock.stockInfo.objectPrefab, scanPoint.position, scanPoint.rotation, scanPoint);
        currentItem.MakePlaced();
        Destroy(currentItem.gameObject, timeToScan);
    }

    private void HandleScanning()
    {
        if(timer > 0)
            return;
        
        timer = timeToScan;

        if(currentItems == null || currentItemIndex >= currentItems.Count)
        {
            FinishScanning();
            return;
        }
        
        // Finished Scanning
        PurchasedItem item = currentItems[currentItemIndex];

        runningTotal += item.purchasePrice;

        currentItemText.text = item.stock.stockInfo.objectName + " - $" + item.purchasePrice.ToString("F2");
        priceText.text = "$" + runningTotal.ToString("F2");

        // Try to move on to next item
        currentItemIndex++;

        if(currentItemIndex >= currentItems.Count)
        {
            // Finished scanning all items, move to payment
            FinishScanning();
            return;
        }

        currentItem = Instantiate(currentItems[currentItemIndex].stock.stockInfo.objectPrefab, scanPoint.position, scanPoint.rotation, scanPoint);
        currentItem.MakePlaced();
        Destroy(currentItem.gameObject, timeToScan);
    }

    private void FinishScanning()
    {
        if(HasCashier)
            timer = cashier.paymentProccesingTime;

        state = CheckoutState.WaitingForPayment;

        currentItemText.text = "Ready for payment!";
    }

    private void CheckoutCustomer()
    {
        StoreManager.instance.CompleteSale(runningTotal);

        HidePrice();

        currentCustomer.CheckoutFinished();

        customersInQueue.Remove(currentCustomer);

        ClearCheckout();
        UpdateQueue();

        if(AudioManager.instance != null)
            AudioManager.instance.PlaySFX(SoundEffectType.Checkout);
    }

    private void ClearCheckout()
    {
        if(HasCashier)
            timer = cashier.customerWaitTime;

        Debug.Log("clearing checkout");

        currentCustomer = null;
        currentItems = null;

        currentItemIndex = 0;
        runningTotal = 0;

        currentItemText.text = "";
        priceText.text = "";

        state = CheckoutState.Idle;
    }

    private bool CanCheckout() 
    { 
        if(customersInQueue.Count == 0)
            return false;

        return Vector3.Distance(customersInQueue[0].transform.position, queuePoint.position) < 0.1f;
    }


    public void QueueCustomer(Customer customer) 
    { 
        customersInQueue.Add(customer);

        UpdateQueue();
    }

    private void UpdateQueue()
    {
        for(int i = 0; i < customersInQueue.Count; i++)
        {
            customersInQueue[i].UpdateQueuePoint(queuePoint.position + (queuePoint.forward * i * spaceBetweenCustomers));
        }
    }

    public Transform GetCashierPoint() => cashierPoint;

    public void SetCashier(Cashier cashierToSet) => cashier = cashierToSet; 
}
