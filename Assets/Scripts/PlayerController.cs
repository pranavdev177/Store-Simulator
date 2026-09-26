using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement Settings")]
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] float lookSpeed;

    [Header("Interaction Settings")]
    [SerializeField] float interactionRange;
    [SerializeField] float throwForce;
    [SerializeField] Transform holdPoint;
    [SerializeField] Transform boxHoldPoint;
    [SerializeField] Transform furniturePoint;
    [SerializeField] float waitToPlaceStock;

    [Header("Camera Settings")]
    [SerializeField] Camera cam;
    [SerializeField] float minLookAngle;
    [SerializeField] float maxLookAngle;

    [Header("Actions")]
    [SerializeField] InputActionReference moveAction;
    [SerializeField] InputActionReference jumpAction;
    [SerializeField] InputActionReference lookAction;
    [SerializeField] InputActionReference leftMouseAction;
    [SerializeField] InputActionReference rightMouseAction;
    [SerializeField] InputActionReference interactAction;
    [SerializeField] InputActionReference furnitureInteractAction;

    [Header("Layermasks")]
    [SerializeField] LayerMask whatIsStock;
    [SerializeField] LayerMask whatIsShelf;
    [SerializeField] LayerMask whatIsStockBox;
    [SerializeField] LayerMask whatIsBin;
    [SerializeField] LayerMask whatIsFurniture;
    [SerializeField] LayerMask whatIsCheckout;

    private CharacterController charCon;
    private float ySpeed;
    private float hRotation, vRotation;

    private IHoldable heldObject;
    private StockObject HeldStock => heldObject as StockObject;
    private StockBoxController HeldBox => heldObject as StockBoxController;

    private float placeStockCounter;

    void Awake()
    {
        charCon = GetComponent<CharacterController>(); 
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if(UIController.instance.isActive)
            return;


        RotateCharacter();

        Vector3 moveAmount = GetHorizontalMovement();
        moveAmount.y = GetVerticalMovement();

        charCon.Move(moveAmount * Time.deltaTime);

        HandleInteraction();
    }

    #region Interaction Management

    private void HandleInteraction()
    {
        if (heldObject == null)
        {
            HandleInteractionWithNoHeldObject();
            return;
        }

        if(heldObject != null)
            HandleInteractionWithAnyHeldObject();
        
        if (HeldStock)
        {
            HandleInteractionWithHeldStock();
            return;
        }

        if(HeldBox)
        {
            HandleInteractionWithHeldBox();
            return;
        }
    }

    private void HandleInteractionWithNoHeldObject()
    {
        if(leftMouseAction.action.WasPressedThisFrame())
        {
            TryPickupObject(); 
            TryInteractWithCheckout();
        }

        if(rightMouseAction.action.WasPressedThisFrame())
            TryPickupShelfedObject();

        if(interactAction.action.WasPressedThisFrame())
        {
            TryInteractWithShelf();
            TryInteractWithUnheldBox();
        }

        if(furnitureInteractAction.action.WasPressedThisFrame())
            TryPickup<FurnitureController>(whatIsFurniture, furniturePoint);
    }

    private void HandleInteractionWithHeldBox()
    {
        if(interactAction.action.WasPressedThisFrame())
            HeldBox.OpenClose();

        if(leftMouseAction.action.IsPressed() && HeldBox.IsOpen())
            TryFastShelfBoxObject();

        if(leftMouseAction.action.WasPressedThisFrame())
            if(HeldBox.HasStock() && HeldBox.IsOpen())
                TryShelfBoxObject();
            else
                TryBinBoxObject();
    }

    private void HandleInteractionWithAnyHeldObject()
    {
        if(rightMouseAction.action.WasPressedThisFrame())
            ThrowHeldObject();
    }

    private void HandleInteractionWithHeldStock()
    {
        if(leftMouseAction.action.WasPressedThisFrame())
            TryShelfObject(); 
    }

    #endregion

    #region Interaction Functions

    private void TryInteractWithCheckout()
    {
        if (!TryRaycast(whatIsCheckout, out RaycastHit hit))
            return;

        hit.collider.GetComponent<Checkout>().PlayerPressedCheckout();
    }

    private void TryFastShelfBoxObject()
    {
        placeStockCounter -= Time.deltaTime;    
        if(placeStockCounter <= 0)
        {
            TryShelfBoxObject();
        }
    }

    private void TryBinBoxObject()
    {
        if (!TryRaycast(whatIsBin, out RaycastHit hit))
            return;

        Destroy(HeldBox.gameObject);
        heldObject = null;


        if(AudioManager.instance != null)
            AudioManager.instance.PlaySFX(SoundEffectType.Trash);
    }

    private void TryShelfBoxObject()
    {
        if (!TryRaycast(whatIsShelf, out RaycastHit hit))
            return;

        HeldBox.PlaceStockOnShelf(hit.collider.GetComponent<ShelfSpaceController>());
        placeStockCounter = waitToPlaceStock;

    }

    private void TryInteractWithUnheldBox()
    {
        if (!TryRaycast(whatIsStockBox, out RaycastHit hit))
            return;

        hit.collider.GetComponent<StockBoxController>().OpenClose();
    }

    private void TryInteractWithShelf()
    {
        if (!TryRaycast(whatIsShelf, out RaycastHit hit))
            return;

        hit.collider.GetComponent<ShelfSpaceController>().StartPriceUpdate();
    }

    private void TryPickupShelfedObject()
    {
        if (!TryRaycast(whatIsShelf, out RaycastHit hit))
            return;

        StockObject stock = hit.collider.GetComponent<ShelfSpaceController>().GetStock();

        if(stock == null)
            return;

        heldObject = stock;
    
        stock.transform.SetParent(holdPoint);
        stock.Pickup();
    }

    private void TryShelfObject()
    {
        if (!TryRaycast(whatIsShelf, out RaycastHit hit))
            return;
       
        ShelfSpaceController shelf = hit.collider.GetComponent<ShelfSpaceController>();
        StockObject stock = HeldStock;

        if(shelf.PlaceStock(stock) && stock)
            heldObject = null;
    }

    private void TryPickupObject()
    {
        if(TryPickup<StockObject>(whatIsStock, holdPoint))
            return;

        TryPickup<StockBoxController>(whatIsStockBox, boxHoldPoint);
    }

    private bool TryPickup<T>(LayerMask layerMask, Transform parent) where T: MonoBehaviour, IHoldable
    {
        if (!TryRaycast(layerMask, out RaycastHit hit))
            return false;

        T item = hit.collider.GetComponent<T>();
        heldObject = item;

        item.transform.SetParent(parent);
        item.Pickup();

        return true;
    }

    private void ThrowHeldObject()
    {
        if(!heldObject.TryThrow(cam.transform.forward * throwForce))
            return;

        heldObject.Release();

        MonoBehaviour heldMono = heldObject as MonoBehaviour;
        heldMono.transform.SetParent(null);

        heldObject = null;
    }

    private bool TryRaycast(LayerMask layerMask, out RaycastHit hit)
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        return Physics.Raycast(ray, out hit, interactionRange, layerMask);
    }
    #endregion

    #region Character Movement and Rotation
    private void RotateCharacter()
    {
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        hRotation += lookInput.x * Time.deltaTime * lookSpeed;
        vRotation -= lookInput.y * Time.deltaTime * lookSpeed;

        vRotation = Mathf.Clamp(vRotation, minLookAngle, maxLookAngle);

        transform.rotation = Quaternion.Euler(0f, hRotation, 0f);
        cam.transform.localRotation = Quaternion.Euler(vRotation, 0f, 0f);
    }

    private Vector3 GetHorizontalMovement()
    {
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();

        Vector3 vMovement = transform.forward * moveInput.y;
        Vector3 hMovement = transform.right * moveInput.x;
        Vector3 moveAmount = hMovement + vMovement;

        if (moveAmount.sqrMagnitude > 1f)
            moveAmount.Normalize();

        return moveAmount * moveSpeed;
    }

    private float GetVerticalMovement()
    {
        if(charCon.isGrounded)
        {
            ySpeed = 0;

            if(jumpAction.action.WasPressedThisFrame())
            {
                ySpeed = jumpForce;

                
                if(AudioManager.instance != null)
                    AudioManager.instance.PlaySFX(SoundEffectType.Jump);
            }
        }

        ySpeed += Physics.gravity.y * Time.deltaTime;

        return ySpeed;
    }
    #endregion

    #region Enable and Disable Actions
    private void OnEnable()
    {
        leftMouseAction.action.Enable();
        rightMouseAction.action.Enable();
        moveAction.action.Enable();
        jumpAction.action.Enable();
        lookAction.action.Enable();
    }

    private void OnDisable()
    {
        leftMouseAction.action.Disable();
        rightMouseAction.action.Disable();
        moveAction.action.Disable();
        jumpAction.action.Disable();
        lookAction.action.Disable();
    }
    #endregion
}
