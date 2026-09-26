using UnityEngine;
using System.Collections.Generic;
using System;

public class DayManager : MonoBehaviour
{
    public static event Action OnWorkingDayStart;
    public static event Action OnDayStart;
    public static event Action OnDayEnd;


    [Header("Day Manager Settings")]
    [SerializeField] float storeDayDurationMinutes  = 100;
    [SerializeField] float openingTime = 9 * 60;
    [SerializeField] float closingTime = 17 * 60;
    [SerializeField] float prepTime = 60;
    [SerializeField] float stopSpawningBeforeClosingSeconds = 30f;

    public static DayManager instance {get; private set;}

    private int currentDay = 0;
    private float currentTime;

    private int lastDisplayedTime;
    private float timeScale;
    private bool dayEnded;

    private readonly List<DayStatistics> history = new();
    public DayStatistics currentDayStats {get; private set;} = new();
    private float StopSpawningTime => closingTime - (30f * timeScale);

    public bool StoreOpen => currentTime >= openingTime && currentTime < closingTime;
    public bool ShouldSpawn => currentTime < StopSpawningTime && StoreOpen;

    void Awake()
    {
        instance = this;
    }


    void Start()
    {
        float storeMinutes = closingTime - openingTime;
        float realSeconds = storeDayDurationMinutes * 60f;

        timeScale = storeMinutes / realSeconds;

        StartNextDay();
    }

    public void StartNextDay()
    {
        OnWorkingDayStart?.Invoke();

        currentDay++;
        currentTime = openingTime - prepTime;
        dayEnded = false;

        currentDayStats = new DayStatistics
        {
            day = currentDay
        };
    }

    private void EndDay()
    {
        OnDayEnd?.Invoke();
        dayEnded = true;

        DayStatistics yesterday = history.Count > 0 ? history[^1] : null;
        history.Add(currentDayStats);

        UIController.instance.ShowDaySummary(
            new DaySummaryData
            {
                day = "Day " + currentDay,
                revenue = currentDayStats.revenue,
                profit = currentDayStats.profit,
                itemsSold = currentDayStats.itemsSold,
                customersServed = currentDayStats.customersServed,
                emptyHanded = currentDayStats.customersLeftEmptyHanded,
                revenueChange = yesterday != null ? currentDayStats.revenue - yesterday.revenue : null,
                profitChange = yesterday != null ? currentDayStats.profit - yesterday.profit : null,
                itemsSoldChange = yesterday != null ? currentDayStats.itemsSold - yesterday.itemsSold : null,
                customersServedChange = yesterday != null ? currentDayStats.customersServed - yesterday.customersServed : null,
                emptyHandedChange = yesterday != null ? currentDayStats.customersLeftEmptyHanded - yesterday.customersLeftEmptyHanded : null,
                customerSatisfactionData = ReviewManager.instance.GetDayReviewData()
            }
        );
    }

    void Update()
    {
        if(dayEnded)
            return;

        UpdateTime();
        UpdateTimeUI();
        CheckStoreClosing();
    }

    private void UpdateTime()
    {
        currentTime += Time.deltaTime * timeScale;
    }

    private void UpdateTimeUI()
    {
        int roundedTime = Mathf.FloorToInt(currentTime / 10f) * 10;

        if(roundedTime != lastDisplayedTime)
        {
            lastDisplayedTime = roundedTime;
            UIController.instance.UpdateTime(GetFormattedTime(roundedTime));
        }
    }

    private void CheckStoreClosing()
    {
        if(currentTime >= closingTime)
        {
            currentTime = closingTime;

            if(CustomerManager.instance.ActiveCustomers == 0)
                EndDay();
        }
    }

    private string GetFormattedTime(float timeToFormat)
    {
        int hours = Mathf.FloorToInt(timeToFormat / 60);
        int minutes = Mathf.FloorToInt(timeToFormat % 60);

        return $"{hours:00}:{minutes:00}";
    }
}

public class DayStatistics
{
    public int day;

    public float revenue;
    public float profit;

    public int customersServed;
    public int customersLeftEmptyHanded;
    public int itemsSold;
}
