using UnityEngine;

public class StockObject : MonoBehaviour, IHoldable
{
    [SerializeField] float moveSpeed;
    public StockInfoSO stockInfo;
    
    [HideInInspector] public bool isPlaced {get; private set;}
    
    private Rigidbody rb;
    private Collider col;

    private bool inBag;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    void Update()
    {
        if(isPlaced)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, moveSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, moveSpeed * Time.deltaTime);
        }

        if(inBag)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, Time.deltaTime * 0.5f);
        }
    }

    public void Pickup()
    {
        rb.isKinematic = true;
        col.enabled = false;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        isPlaced = false;


        if(AudioManager.instance != null)
            AudioManager.instance.PlaySFX(SoundEffectType.ItemPickup);
    }

    public void Release()
    {
        rb.isKinematic = false;
        col.enabled = true;
    }

    public void MakePlaced()
    {
        isPlaced = true;

        rb.isKinematic = true;
        col.enabled = false;
    }

    public bool TryThrow(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);


        if(AudioManager.instance != null)
            AudioManager.instance.PlaySFX(SoundEffectType.ItemThrow);

        return true;
    }

    public void PlaceInBox()
    {
        rb.isKinematic = true;
        col.enabled = false;
    }

    public void PlaceInBag()
    {
        inBag = true;

        MakePlaced();
    }
}


