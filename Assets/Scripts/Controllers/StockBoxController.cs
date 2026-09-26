using System.Collections.Generic;
using UnityEngine;

public class StockBoxController : MonoBehaviour, IHoldable
{
    [Header("Placement Points")]
    [SerializeField] List<Transform> bigDrinkPoints;
    [SerializeField] List<Transform> cerealPoints, tubeChipPoints, fruitPoints, largeFruitPoints;

    [Header("Flaps")]
    [SerializeField] Transform flap1;
    [SerializeField] Transform flap2;
    [SerializeField] Vector3 flap1OpenPosition;
    [SerializeField] Vector3 flap1ClosedPosition;
    [SerializeField] Vector3 flap2OpenPosition;
    [SerializeField] Vector3 flap2ClosedPosition;

    private Quaternion flap1TargetPosition;
    private Quaternion flap2TargetPosition;

    [Space]
    [SerializeField] float moveSpeed = 5f;

    [SerializeField] StockInfoSO info;
    public bool testFill;

    private List<StockObject> stockInBox = new List<StockObject>();

    private Rigidbody rb;
    private Collider col;

    private bool isHeld;
    private bool isOpen = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        
        flap1TargetPosition = Quaternion.Euler(flap1ClosedPosition);
        flap2TargetPosition = Quaternion.Euler(flap2ClosedPosition);
    }

    void Update()
    {
        if(testFill)
        {
            testFill = false;
            SetupBox(info);
        }

        if(isHeld)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, moveSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, moveSpeed * Time.deltaTime);
        }

        flap1.localRotation = Quaternion.Slerp(flap1.localRotation, flap1TargetPosition, moveSpeed * Time.deltaTime);
        flap2.localRotation = Quaternion.Slerp(flap2.localRotation, flap2TargetPosition, moveSpeed * Time.deltaTime);

    }

    #region Placing Items in Box
    public void SetupBox(StockInfoSO stockType)
    {
        info = stockType;

        List<Transform> activePoints = new List<Transform>();

        activePoints.AddRange(GetPlacementPoints(info));

        if(stockInBox.Count == 0)
        {
            for(int i = 0; i < activePoints.Count; i++)
            {
                StockObject stock = Instantiate(stockType.objectPrefab, activePoints[i]);
                stock.transform.localPosition = Vector3.zero;
                stock.transform.localRotation = Quaternion.identity;

                stockInBox.Add(stock);

                stock.PlaceInBox();
            }
        }
    }

    private List<Transform> GetPlacementPoints(StockInfoSO info)
    {
        switch(info.stockType){
            case StockInfoSO.StockType.bigDrink: return bigDrinkPoints;
            case StockInfoSO.StockType.cereal: return cerealPoints;
            case StockInfoSO.StockType.chipsTube: return tubeChipPoints;
            case StockInfoSO.StockType.fruit: return fruitPoints;
            case StockInfoSO.StockType.fruitLarge: return largeFruitPoints;
        }

        Debug.LogError($"Unhandled stock type: {info.stockType}");
        return null;    
    }
    #endregion

    public void Pickup()
    {
        rb.isKinematic = true;

        isHeld = true;

        if(AudioManager.instance != null)
            AudioManager.instance.PlaySFX(SoundEffectType.BoxGrab);
    }

    public void Release()
    {
        rb.isKinematic = false;

        col.enabled = true;

        isHeld = false;
    }

    public bool TryThrow(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);

        if(AudioManager.instance != null)
            AudioManager.instance.PlaySFX(SoundEffectType.BoxDrop);

        return true;
    }

    public void OpenClose()
    {
        isOpen = !isOpen;

        flap1TargetPosition = Quaternion.Euler(isOpen ? flap1OpenPosition:flap1ClosedPosition);
        flap2TargetPosition = Quaternion.Euler(isOpen ? flap2OpenPosition:flap2ClosedPosition);

        if(AudioManager.instance != null)
            AudioManager.instance.PlaySFX(SoundEffectType.BoxOpen);
    }

    public void PlaceStockOnShelf(ShelfSpaceController shelf)
    {
        if(stockInBox.Count == 0 || shelf == null)
            return;

        shelf.PlaceStock(stockInBox[stockInBox.Count - 1]);

        if (stockInBox[stockInBox.Count - 1].isPlaced)
        {
            stockInBox.RemoveAt(stockInBox.Count - 1);
        }
    }

    public int GetStockAmount(StockInfoSO product) => GetPlacementPoints(product).Count;

    public bool IsOpen() => isOpen;

    public bool HasStock() => stockInBox.Count > 0;
}