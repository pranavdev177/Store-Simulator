using UnityEngine;
using UnityEngine.UIElements;

public class DaySummaryUI : MonoBehaviour
{
    [Header("Headers")]
    [SerializeField] VisualElementReference<VisualElement> panel;
    [SerializeField] VisualElementReference<Label> dayLabel;
    [SerializeField] VisualElementReference<Button> nextDayButton; 

    [Header("Basic Info")]
    [SerializeField] VisualElementReference<Label> revenueAmount;
    [SerializeField] VisualElementReference<Label> revenueChange;
    [SerializeField] VisualElementReference<Label> profitAmount;
    [SerializeField] VisualElementReference<Label> profitChange;
    [SerializeField] VisualElementReference<Label> customersServedAmount;
    [SerializeField] VisualElementReference<Label> customersServedChange;
    [SerializeField] VisualElementReference<Label> itemsSoldAmount;
    [SerializeField] VisualElementReference<Label> itemsSoldChange;
    [SerializeField] VisualElementReference<Label> emptyHandedAmount;
    [SerializeField] VisualElementReference<Label> emptyHandedChange;

    [Header("Customer Satisfaction Info")]
    [SerializeField] VisualElementReference<VisualElement> satisfactionPercentageBar;
    [SerializeField] VisualElementReference<Label> veryHappyCustomers;
    [SerializeField] VisualElementReference<Label> happyCustomers;
    [SerializeField] VisualElementReference<Label> nuetralCustomers;
    [SerializeField] VisualElementReference<Label> unhappyCustomers;


    private VisualElement _panel;
    private Label _dayLabel;

    private Button _nextDayButton;

    private Label _revenueAmount;
    private Label _revenueChange;

    private Label _profitAmount;
    private Label _profitChange;

    private Label _customersServedAmount;
    private Label _customersServedChange;

    private Label _itemsSoldAmount;
    private Label _itemsSoldChange;

    private Label _emptyHandedAmount;
    private Label _emptyHandedChange;

    private VisualElement _satisfactionPercentageBar;

    private Label _veryHappyCustomers;
    private Label _happyCustomers;
    private Label _neutralCustomers;
    private Label _unhappyCustomers;

    void Start()
    {
        Hide();
    }

    private void OnNextDayClicked()
    {
        Hide();
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        DayManager.instance.StartNextDay();
    }

    public void Show(DaySummaryData data)
    {
        _panel.visible = true;

        _dayLabel.text = data.day;

        _revenueAmount.text = $"${data.revenue:F2}";
        _revenueChange.text = $"{data.revenueChange:+0.00;-0.00;0.00}";

        _profitAmount.text = $"${data.profit:F2}";
        _profitChange.text = $"{data.profitChange:+0.00;-0.00;0.00}";

        _customersServedAmount.text = data.customersServed.ToString();
        _customersServedChange.text = data.customersServedChange?.ToString("+0;-0;0");

        _itemsSoldAmount.text = data.itemsSold.ToString();
        _itemsSoldChange.text = data.itemsSoldChange?.ToString("+0;-0;0");

        _emptyHandedAmount.text = data.emptyHanded.ToString();
        _emptyHandedChange.text = data.emptyHandedChange?.ToString("+0;-0;0");;

        SetCustomerBreakdown(data.customerSatisfactionData);
        SetCustomerSatisfaction(data.customerSatisfactionData.averageSatisfaction);
    }


    public void Hide()
    {
        _panel.visible = false;
    }

    private void SetCustomerSatisfaction(float satisfactionPercent)
    {
        Debug.Log(satisfactionPercent);
        satisfactionPercent = Mathf.Clamp(satisfactionPercent, 0f, 100f);

        _satisfactionPercentageBar.style.width = new Length(satisfactionPercent, LengthUnit.Percent);

        if (satisfactionPercent >= 85f)
        {
            _satisfactionPercentageBar.style.backgroundColor = (Color) new Color32(70, 180, 80, 255);
        }
        else if (satisfactionPercent >= 70f)
        {
            _satisfactionPercentageBar.style.backgroundColor = (Color) new Color32(245, 190, 50, 255);
        }
        else if (satisfactionPercent >= 55f)
        {
            _satisfactionPercentageBar.style.backgroundColor = (Color) new Color32(240, 150, 50, 255);
        }
        else
        {
            _satisfactionPercentageBar.style.backgroundColor = (Color) new Color32(210, 70, 65, 255);
        }
    }

    private void SetCustomerBreakdown(DayReviewData data)
    {
        _veryHappyCustomers.text = $"{(float)data.veryHappyCustomers / data.customersToday:P0} ({data.veryHappyCustomers})";
        _happyCustomers.text = $"{(float)data.happyCustomers / data.customersToday:P0} ({data.happyCustomers})";
        _neutralCustomers.text = $"{(float)data.neutralCustomers / data.customersToday:P0} ({data.neutralCustomers})";
        _unhappyCustomers.text = $"{(float)data.unhappyCustomers / data.customersToday:P0} ({data.unhappyCustomers})";
    }

    private void OnEnable()
    {
        panel.RegisterReferenceResolvedCallback(x => _panel = x);

        dayLabel.RegisterReferenceResolvedCallback(x => _dayLabel = x);

        revenueAmount.RegisterReferenceResolvedCallback(x => _revenueAmount = x);
        revenueChange.RegisterReferenceResolvedCallback(x => _revenueChange = x);

        profitAmount.RegisterReferenceResolvedCallback(x => _profitAmount = x);
        profitChange.RegisterReferenceResolvedCallback(x => _profitChange = x);

        customersServedAmount.RegisterReferenceResolvedCallback(x => _customersServedAmount = x);
        customersServedChange.RegisterReferenceResolvedCallback(x => _customersServedChange = x);

        itemsSoldAmount.RegisterReferenceResolvedCallback(x => _itemsSoldAmount = x);
        itemsSoldChange.RegisterReferenceResolvedCallback(x => _itemsSoldChange = x);

        emptyHandedAmount.RegisterReferenceResolvedCallback(x => _emptyHandedAmount = x);
        emptyHandedChange.RegisterReferenceResolvedCallback(x => _emptyHandedChange = x);

        satisfactionPercentageBar.RegisterReferenceResolvedCallback(x => _satisfactionPercentageBar = x);

        veryHappyCustomers.RegisterReferenceResolvedCallback(x => _veryHappyCustomers = x);
        happyCustomers.RegisterReferenceResolvedCallback(x => _happyCustomers = x);
        nuetralCustomers.RegisterReferenceResolvedCallback(x => _neutralCustomers = x);
        unhappyCustomers.RegisterReferenceResolvedCallback(x => _unhappyCustomers = x);

        nextDayButton.RegisterReferenceResolvedCallback(button =>
        {
            _nextDayButton = button;
            _nextDayButton.clicked += OnNextDayClicked;
        });
    }

    private void OnDisable()
    {
        if (_nextDayButton != null)
            _nextDayButton.clicked -= OnNextDayClicked;
    }
}

public class DaySummaryData
{
    public string day;

    public float revenue;
    public float? revenueChange;

    public float profit;
    public float? profitChange;

    public int customersServed;
    public int? customersServedChange;

    public int itemsSold;
    public int? itemsSoldChange;

    public int emptyHanded;
    public int? emptyHandedChange;

    public DayReviewData customerSatisfactionData;
}

