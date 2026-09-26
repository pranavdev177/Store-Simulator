using UnityEngine;

[System.Serializable]
public class CustomerReview
{
    const int BASE_PRICE_PENALTY = 8;
    const int BASE_MISSING_ITEM_PENALTY = 5;
    const int BASE_LONG_WAIT_PENALTY = 10;

    const int BASE_SHORT_WAIT_REWARD = 10;
    const int BASE_FULL_STOCK_REWARD = 12;

    public int score = 70;

    public int overpricedItems;
    public int itemsNotFound;
    public int itemsFound;
    public float checkoutWait;

    private CustomerShoppingProfileSO profile;

    public CustomerReview(CustomerShoppingProfileSO profile)
    {
        this.profile = profile;
    }

    public void ItemFound()
    {
        itemsFound++;
    }

    public void ItemNotFound()
    {
        itemsNotFound++;
        SubtractScore(BASE_MISSING_ITEM_PENALTY * profile.stockImportance);
    }

    public void OverpricedItem(float severity)
    {
        severity = Mathf.Clamp01(severity);

        overpricedItems++;
        SubtractScore(BASE_PRICE_PENALTY * profile.priceImportance * severity);
    }

    public void CheckoutFinishedIn(float waitTime)
    {
        checkoutWait = waitTime;

        if(waitTime > profile.acceptableCheckoutWaitTime)
            SubtractScore(BASE_LONG_WAIT_PENALTY * profile.serviceImportance);
        
        else if(waitTime < profile.excellentCheckoutWaitTime)
            AddScore(BASE_SHORT_WAIT_REWARD * profile.serviceImportance);

        else if(waitTime < profile.idealCheckoutWaitTime)
            AddScore(BASE_SHORT_WAIT_REWARD * profile.serviceImportance * 0.5f);
    }

    public void FinalizeReview()
    {
        float foundRatio = (float)itemsFound / (itemsFound + itemsNotFound);

        if(itemsFound == 0)
        {
            SubtractScore(BASE_MISSING_ITEM_PENALTY * profile.stockImportance * 4f);
            score = Mathf.Min(score, 25);
        }

        else if (itemsNotFound == 0 || foundRatio >= 1f)
            AddScore(BASE_FULL_STOCK_REWARD * profile.stockImportance);

        else if (foundRatio >= 0.8f)
            AddScore(BASE_FULL_STOCK_REWARD * profile.stockImportance * 0.7f);

        else if (foundRatio >= 0.6f || itemsNotFound == 1)
            AddScore(BASE_FULL_STOCK_REWARD * profile.stockImportance * 0.4f);
    }

    public string GetExitMessage()
    {
        if (score >= 90)
            return "Perfect! They had everything I needed.";

        if (score >= 75)
            return "Not bad! I'll come back here.";

        if (score >= 60)
            return "I found a few things at least.";

        if (score >= 40)
            return "That shopping trip could have gone better.";

        return "I'm not coming back here again.";
    }

    public EmotionType GetEmotion()
    {
        if (score >= 90)
            return EmotionType.VeryPositive;

        if (score >= 75)
            return EmotionType.SlightlyPositive;

        if (score >= 60)
            return EmotionType.Nuetral;

        if (score >= 40)
            return EmotionType.SlightlyNegative;

        return EmotionType.VeryNegative;
    }

    private void AddScore(float amount)
    {
        score += Mathf.RoundToInt(amount);
        score = Mathf.Clamp(score, 0, 100);
    }

    private void SubtractScore(float amount)
    {
        score -= Mathf.RoundToInt(amount);
        score = Mathf.Clamp(score, 0, 100);
    }

}
