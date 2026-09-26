using System.Collections.Generic;
using UnityEngine;

public class BuyMenuController : MonoBehaviour
{
    [SerializeField] GameObject stockPanel, furniturePanel;
    [Header("Stock Frame")]
    [SerializeField] Transform stockFrameGrid;
    [SerializeField] GameObject stockFramePrefab;
    [SerializeField] List<StockInfoSO> stockObjects;

    [Header("Furniture Panel")]
    [SerializeField] Transform furnitureFrameGrid;
    [SerializeField] GameObject furnitureFramePrefab;
    [SerializeField] List<FurnitureInfoSO> furnitureObjects;

    void Start()
    {
        PopulateStockPanel();
        PopulateFurniturePanel();
    }

    public void OpenStockPanel()
    {
        stockPanel.SetActive(true);
        furniturePanel.SetActive(false);
    }

    public void OpenFurniturePanel()
    {
        stockPanel.SetActive(false);
        furniturePanel.SetActive(true);
    }

    private void PopulateStockPanel()
    {
        foreach(StockInfoSO stockObject in stockObjects)
        {
            GameObject stockFrame = Instantiate(stockFramePrefab, parent: stockFrameGrid);
            stockFrame.GetComponent<ProductUI>().Setup(stockObject);
        }
    }

    private void PopulateFurniturePanel()
    {
        foreach(FurnitureInfoSO furniture in furnitureObjects)
        {
            GameObject furnitureFrame = Instantiate(furnitureFramePrefab, parent: furnitureFrameGrid);
            furnitureFrame.GetComponent<FurnitureUI>().Setup(furniture);
        }
    }
}
