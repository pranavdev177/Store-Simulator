using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        transform.forward = -cam.transform.forward;
    }
}