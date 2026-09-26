using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

enum CustomerState
{
    Entering,
    GoingToShelf,
    BrowsingAtShelf,
    WaitingAfterGrab,
    CheckingOut,
    Leaving
}

public class PurchasedItem
{
    public StockObject stock;
    public float purchasePrice;
}

public class Customer : MonoBehaviour
{
    [SerializeField] GameObject shoppingBag;

    private NavMeshAgent agent;
    private Animator anim;
    private SpeechManager speechManager;

    private CustomerSettings settings;
    private CustomerState currentState;

    private FurnitureController currentShelfCase;

    private float waitTimer;

    private List<ShoppingListItem> itemsLeftToBuy = new();
    private List<PurchasedItem> stockInBag = new List<PurchasedItem>();
    private float totalSpent;

    private Dictionary<CustomerState, System.Action> stateHandlers;

    private CustomerReview review;

    private bool complained;
    private bool gotAngry;
    private bool hasVisitedShelf;
    
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        speechManager = GetComponent<SpeechManager>();

        stateHandlers = new()
        {
            {CustomerState.Entering, HandleEnteringState},
            {CustomerState.GoingToShelf, HandleGoingToShelfState},
            {CustomerState.BrowsingAtShelf, HandleBrowsingState},
            {CustomerState.WaitingAfterGrab, HandleWaitingAfterGrabState},
            {CustomerState.CheckingOut, HandleCheckingOutState},
            {CustomerState.Leaving, HandleLeavingState},
        };
    }

    void Update()
    {
        anim.SetFloat("Speed", agent.velocity.magnitude);

       stateHandlers[currentState]();
    }

    #region Handle States
    private void HandleEnteringState()
    {
        if(!ReachedDestination())
            return;

        if(!StoreManager.instance.HasShelf())
        {
            StartLeaving();
            return;
        }

        shoppingBag.SetActive(true);
        
        TryMoveToNextShelf();
    }

    private void HandleGoingToShelfState()
    {
        if(!ReachedDestination())
            return;

        hasVisitedShelf = true;

        waitTimer = Random.Range(settings.minBrowseTime, settings.maxBrowseTime);
        currentState = CustomerState.BrowsingAtShelf;
    }

    private void HandleBrowsingState()
    {
        FaceShelf();
        waitTimer -= Time.deltaTime;

        if(waitTimer > 0)
            return;

        // Try to get stock from shelf
        if(TryGrabStock())
        {
            currentState = CustomerState.WaitingAfterGrab;
            return;
        }        

        // Finished browsing, and can go to next shelf
        FinishBrowsing();
    }

    private void HandleWaitingAfterGrabState()
    {
        waitTimer -= Time.deltaTime;

        if (waitTimer > 0)
            return;

        FinishBrowsing();
    }

    private void HandleCheckingOutState()
    {
        waitTimer += Time.deltaTime;

        if(!complained && waitTimer > settings.profile.idealCheckoutWaitTime)
        {
            complained = true;
            speechManager.Say("This line is taking a while...", EmotionType.Nuetral, 4f);
        }

        if(!gotAngry && waitTimer > settings.profile.acceptableCheckoutWaitTime)
        {
            gotAngry = true;
            speechManager.Say("Seriously?! I've been waiting forever!", EmotionType.Negative, 6f);
        }
    }

    private void HandleLeavingState()
    {
         if(ReachedDestination())
            Destroy(gameObject);
    }

    #endregion

    #region Navigation

    private void TryMoveToNextShelf()
    {
        currentShelfCase?.ReleaseLookPoint(this);

        for (int i = 0; i < itemsLeftToBuy.Count; i++)
        {
            FurnitureController shelf = StoreManager.instance.FindShelfWithProduct(itemsLeftToBuy[i].product);

            if (shelf != null)
            {
                GoToShelf(shelf);
                return;
            }

            // Item doesn't exist anywhere, remove it.
            review.ItemNotFound();

            itemsLeftToBuy.RemoveAt(i);
            i--;
        }

        // Could not find any items left to buy, Finish Shopping
        if (!hasVisitedShelf)
        {
            GoToShelf(StoreManager.instance.GetRandomShelf());
            return;
        }

        FinishShopping();
    }

   private void GoToShelf(FurnitureController shelf)
    {
        currentShelfCase = shelf;
        currentState = CustomerState.GoingToShelf;
        agent.SetDestination(currentShelfCase.ReserveLookPoint(this).position);
    }

    private bool ReachedDestination()
    {
        if (agent.pathPending)
            return false;

        return agent.remainingDistance <= agent.stoppingDistance;
    }

    #endregion

    #region Shopping

    private bool TryGrabStock()
    {
        if(itemsLeftToBuy.Count == 0)
            return false;

        ShoppingListItem item = itemsLeftToBuy[0];

        if(!WillBuy(item.product))
        {
            itemsLeftToBuy.Remove(item);
            return false;
        }

        ShelfSpaceController shelf = currentShelfCase.GetShelfWithProduct(item.product);

        if(shelf == null)
            return false;

        int desiredAmount = item.amount; //Mathf.Min(item.amount,Random.Range(1, stock.stockInfo.maxPickupAmount + 1));
        List<StockObject> stockTaken = shelf.TakeStock(desiredAmount);

        if(stockTaken.Count <= 0)
            return false;

        foreach(StockObject stock in stockTaken)
            AddToBag(stock);

        item.amount -= stockTaken.Count;
        if (item.amount <= 0)
            itemsLeftToBuy.RemoveAt(0);

        review.ItemFound();

        return true;
    }

    private void AddToBag(StockObject stock)
    {
        stock.transform.SetParent(shoppingBag.transform);

        PurchasedItem itemBought = new PurchasedItem{
            stock = stock,
            purchasePrice = stock.stockInfo.currentPrice
        };

        StoreManager.instance.ItemBought(itemBought);
        StockManager.instance.ItemBought();

        stockInBag.Add(itemBought);
        stock.PlaceInBag();    

        totalSpent += stock.stockInfo.currentPrice;
    }

    private void FinishBrowsing()
    {
        agent.updateRotation = true;

        if(itemsLeftToBuy.Count > 0)
            TryMoveToNextShelf(); 
        else
            FinishShopping();
    }

    private bool WillBuy(StockInfoSO stock)
    {
        float garunteedPrice = stock.price * settings.profile.guarunteedPriceTolerance;;
        float maxPrice = stock.price * settings.profile.maxPriceTolerance;
        float severity = Mathf.InverseLerp(garunteedPrice, maxPrice, stock.currentPrice);

        if(stock.currentPrice <= garunteedPrice)
            return true;

        if(stock.currentPrice >= maxPrice)
        {
            review.OverpricedItem(severity);
            speechManager.Say(stock.objectName + " costs too much!!", EmotionType.Negative, 4f);

            return false;
        }

        float chance = 1f - (stock.currentPrice - garunteedPrice) / (maxPrice - garunteedPrice);
        bool buys = Random.value < chance;

        if(!buys)
            review.OverpricedItem(severity);

        return buys;
    }

    private void FinishShopping()
    {
        if(stockInBag.Count > 0)
            StartQueueing();
        else
        {
            speechManager.Say("I couldn't find anything I wanted!", EmotionType.VeryNegative, 6f);
            CustomerManager.instance.CustomerLeftEmptyHanded();    

            StartLeaving();
        }
    }

    private void FaceShelf()
    {
        agent.updateRotation = false;

        Vector3 direction = currentShelfCase.transform.position - transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                360f * Time.deltaTime);
        }
    }

    #endregion

    #region State Transitions

    private void StartQueueing()
    {
        complained = false;
        gotAngry = false;

        StoreManager.instance.GetCheckout().QueueCustomer(this);

        waitTimer = 0;
        currentState = CustomerState.CheckingOut;
    }
    
    private void StartLeaving()
    {
        review.FinalizeReview();
        ReviewManager.instance.AddCustomerReview(review);

        currentState = CustomerState.Leaving;

        CustomerManager.instance.CustomerLeftStore(this);
        Vector3 exitPoint = CustomerManager.instance.GetRandomSpawnPoint().position;
        agent.SetDestination(exitPoint);
    }

    #endregion

    #region Public Functions

    public void UpdateQueuePoint(Vector3 newPoint)
    {
        agent.SetDestination(newPoint);
    }

    public float GetTotalAmountSpent() => totalSpent;

    public List<PurchasedItem> GetItemsInBag() => stockInBag;

    public void CheckoutFinished()
    {
        review.CheckoutFinishedIn(waitTimer);
        CustomerManager.instance.CustomerServed();       

        speechManager.Say(review.GetExitMessage(), review.GetEmotion(), 6f);

        StartLeaving();
    }

    public void SetupCustomer(CustomerSettings settings)
    {
        this.settings = settings;

        itemsLeftToBuy = this.settings.shoppingList
            .ConvertAll(item => new ShoppingListItem
            {
                product = item.product,
                amount = item.amount
            });

        review = new CustomerReview(settings.profile);
            
        currentState = CustomerState.Entering;

        agent.speed = Random.Range(settings.profile.minWalkingSpeed, settings.profile.maxWalkingSpeed);

        agent.Warp(this.settings.spawnPoint.position);
        agent.SetDestination(this.settings.storePoint.position);
    }

    #endregion
}
