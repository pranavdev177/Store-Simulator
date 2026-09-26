using UnityEngine;

public interface IHoldable 
{
    public void Pickup();
    public void Release();
    public bool TryThrow(Vector3 force);
}
