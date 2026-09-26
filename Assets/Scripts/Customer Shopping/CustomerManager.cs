using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ShoppingListItem
{
    public StockInfoSO product;
    public int amount;
}

public class CustomerSettings
{
    public Transform spawnPoint;
    public Transform storePoint;

    public float minBrowseTime;
    public float maxBrowseTime;
    public float waitAfterGrabbingStock;

    public List<ShoppingListItem> shoppingList;
    public CustomerShoppingProfileSO profile;
}

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager instance {get; private set;}

    [Header("Spawning Customors")]
    [SerializeField] List<Customer> customersToSpawn = new List<Customer>();    
    [SerializeField] float minTimeBetweenCustomers, maxTimeBetweenCustomers;

    [Header("Points")]
    [SerializeField] List<Transform> spawnPoints;
    [SerializeField] Transform storePoint;
    
    [Header("Browsing Behavior")]
    [SerializeField] float minBrowseTime = 1.5f;
    [SerializeField] float maxBrowseTime = 3f;
    [SerializeField] float waitAfterGrabbingStock = 0.5f;
    [SerializeField] CustomerShoppingProfileSO shoppingProfile;

    private readonly List<Customer> activeCustomers = new();
    private float spawnCounter;

    public int ActiveCustomers => activeCustomers.Count;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (!DayManager.instance.ShouldSpawn)
            return;

        spawnCounter -= Time.deltaTime;

        if(spawnCounter <= 0)
            SpawnCustomer();
    }

    private void SpawnCustomer()
    {
        CustomerSettings settings = new CustomerSettings
        {
            spawnPoint = GetRandomSpawnPoint(),
            storePoint = storePoint,
            minBrowseTime = minBrowseTime,
            maxBrowseTime = maxBrowseTime,
            waitAfterGrabbingStock = waitAfterGrabbingStock,
            shoppingList = ShoppingListGenerator.instance.GenerateList(shoppingProfile),
            profile = shoppingProfile
        };

        Customer customer = Instantiate(customersToSpawn[Random.Range(0, customersToSpawn.Count)]);
        customer.SetupCustomer(settings);

        activeCustomers.Add(customer);
        spawnCounter = Random.Range(minTimeBetweenCustomers, maxTimeBetweenCustomers);
    }

    public Transform GetRandomSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }

    public void CustomerServed() => DayManager.instance.currentDayStats.customersServed++;
    public void CustomerLeftEmptyHanded() => DayManager.instance.currentDayStats.customersLeftEmptyHanded++;
    public void CustomerLeftStore(Customer customer) => activeCustomers.Remove(customer);

}
