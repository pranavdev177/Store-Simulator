using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    public static UIController instance;
    [SerializeField] InputActionReference closeUIAction;
    [SerializeField] InputActionReference menuScreenUIAction;
    [SerializeField] TMP_Text moneyValueText;
    [SerializeField] TMP_Text timeText;

    [Header("Update Price")]
    [SerializeField] GameObject updatePricePanel;
    [SerializeField] TMP_Text basePriceText, currentPriceText;
    [SerializeField] TMP_InputField priceInput;

    [Header("Buy Menu")]
    [SerializeField] GameObject buyMenuScreen;

    [Header("Day Summary Panel")]
    [SerializeField] DaySummaryUI daySummaryUI;


    public bool updatePanelOpen => updatePricePanel.activeSelf;
    public bool buyScreenOpen => buyMenuScreen.activeSelf;

    public bool isActive => updatePanelOpen || buyScreenOpen;


    private StockInfoSO activeStockInfo;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if(menuScreenUIAction.action.WasPressedThisFrame())
            OpenCloseBuyMenu();

        if(closeUIAction.action.WasPressedThisFrame())
            CloseUpdatePrice();
    }

    #region Update Price

    public void OpenUpdatePrice(StockInfoSO stockInfo)
    {
        updatePricePanel.SetActive(true);
        activeStockInfo = stockInfo;

        Cursor.lockState = CursorLockMode.None;

        basePriceText.text = "$" + stockInfo.price.ToString("F2");
        currentPriceText.text = "$" + stockInfo.currentPrice.ToString("F2");

        priceInput.text = "";
    }

    public void CloseUpdatePrice()
    {
        updatePricePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ApplyPriceUpdate()
    {
        activeStockInfo.currentPrice = float.Parse(priceInput.text);
        StockManager.instance.UpdatePrice(activeStockInfo, activeStockInfo.currentPrice);

        CloseUpdatePrice();
    }
    #endregion

    public void UpdateMoney(float currentMoney)
    {
       moneyValueText.text = "$" + currentMoney.ToString("F2"); 
    }

    public void UpdateTime(string time)
    {
        timeText.text = time;
    }

    public void OpenCloseBuyMenu()
    {
        buyMenuScreen.SetActive(!buyScreenOpen);
        Cursor.lockState = buyScreenOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void ShowDaySummary(DaySummaryData dayData)
    {
        daySummaryUI.Show(dayData);

        Cursor.lockState = CursorLockMode.None;
    }
}
