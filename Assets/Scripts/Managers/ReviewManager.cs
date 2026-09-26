using System.Collections.Generic;
using UnityEngine;

public class ReviewManager : MonoBehaviour
{
    public static ReviewManager instance {get; private set;}

    private int customersToday;
    private int veryHappyCustomers;
    private int happyCustomers;
    private int neutralCustomers;
    private int unhappyCustomers;

    private float averageSatisfaction;
    private float totalScore;

    List<CustomerReview> reviews = new();

    void Awake()
    {
        instance = this;
    }

    public void AddCustomerReview(CustomerReview review)
    {
        customersToday++;
        totalScore += review.score;
        averageSatisfaction = totalScore / customersToday;

        if(review.score >= 90)
            veryHappyCustomers++;

        else if (review.score >= 75)
            happyCustomers++;

        else if (review.score >= 60)
            neutralCustomers++;

        else
            unhappyCustomers++;

        reviews.Add(review);
    }

    public DayReviewData GetDayReviewData()
    {
        return new DayReviewData
        {
            customersToday = customersToday,
            averageSatisfaction = averageSatisfaction,
            veryHappyCustomers = veryHappyCustomers,
            happyCustomers = happyCustomers,
            neutralCustomers = neutralCustomers,
            unhappyCustomers = unhappyCustomers
        };
    }
}

public class DayReviewData
{
    public int customersToday;
    public float averageSatisfaction;

    public int veryHappyCustomers;
    public int happyCustomers;
    public int neutralCustomers;
    public int unhappyCustomers;
}