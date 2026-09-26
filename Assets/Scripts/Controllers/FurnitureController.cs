using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FurnitureController : MonoBehaviour, IHoldable
{
    [SerializeField] GameObject mainObject, placingObject;
    [SerializeField] FurnitureInfoSO furnitureInfo;
    [SerializeField] List<Transform> lookPoints;

    [Header("Collision Detection")]
    [SerializeField] LayerMask cantPlaceOnLayer;
    [SerializeField] private Vector3 boxHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);
    [SerializeField] private Vector3 centerOffset; 

    private Dictionary<Customer, Transform> reservedPoints = new();

    private Collider col;
    private List<ShelfSpaceController> shelves;

    void Awake()
    {
        col = GetComponent<Collider>();
        shelves = GetComponentsInChildren<ShelfSpaceController>().ToList();
    }

    void Start()
    {
        if(shelves.Count > 0)
            StoreManager.instance.RegisterShelvingCase(this);
    }

    private bool CanBePlaced()
    {
        col.enabled = true;

        Collider[] hits = Physics.OverlapBox(
            col.bounds.center,
            col.bounds.extents,
            transform.rotation,
            cantPlaceOnLayer
        );

        foreach (Collider hit in hits)
        {
            if (hit != col)
            {
                col.enabled = false;
                return false;
            }
        }

        col.enabled = false;
        return true;
    }

    public void Pickup()
    {
        mainObject.SetActive(false);
        placingObject.SetActive(true);

        col.enabled = false;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity; 

        if(AudioManager.instance != null)
            AudioManager.instance.PlaySFX(SoundEffectType.FurniturePickup);
    }

    public void Release()
    {
        mainObject.SetActive(true);
        placingObject.SetActive(false);

        col.enabled = true;
    }

    public bool TryThrow(Vector3 force)
    {
        if(!CanBePlaced())
            return false;

        mainObject.SetActive(true);
        placingObject.SetActive(false);

        col.enabled = true;

        if(AudioManager.instance != null)
            AudioManager.instance.PlaySFX(SoundEffectType.FurniturePlace);

        return true;
    }

    public bool CanHold(StockInfoSO stock)
    {
        return furnitureInfo.allowedStockTypes.Contains(stock.stockType);
    }

    public Transform ReserveLookPoint(Customer customer)
    {
        if (reservedPoints.TryGetValue(customer, out Transform point))
            return point;

        foreach (Transform lookPoint in lookPoints)
        {
            if(!reservedPoints.ContainsValue(lookPoint))
            {
                reservedPoints.Add(customer, lookPoint);
                return lookPoint;
            }
        }

        // All occupied, just return a random one
        return lookPoints[Random.Range(0, lookPoints.Count)];    
    }

    public void ReleaseLookPoint(Customer customer)
    {
        reservedPoints.Remove(customer);
    }

    public ShelfSpaceController GetShelfWithProduct(StockInfoSO product)
    {
        foreach(ShelfSpaceController shelf in shelves)
        {
            if(shelf.GetInfo() == product)
                return shelf;
        }

        return null;
    }

    public bool HasProduct(StockInfoSO productToCheck)
    {
        foreach(ShelfSpaceController shelf in shelves)
        {
            if(shelf.GetInfo() == productToCheck)
                return true;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + centerOffset, boxHalfExtents * 2);
    }
}
