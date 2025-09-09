using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    [Header("Billboard Settings")]
    public bool freezeXRotation = true;
    public bool freezeZRotation = true;
    public bool useMainCamera = true;
    
    private Camera targetCamera;
    
    void Start()
    {
        if (useMainCamera)
        {
            targetCamera = Camera.main;
        }
        else
        {
            targetCamera = FindObjectOfType<Camera>();
        }
        
        if (targetCamera == null)
        {
        }
    }
    
    void LateUpdate()
    {
        if (targetCamera == null) return;
        
        Vector3 targetPosition = targetCamera.transform.position;
        Vector3 direction = targetPosition - transform.position;
        
        if (freezeXRotation)
            direction.y = 0;
            
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            if (freezeXRotation && freezeZRotation)
            {
                targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
            }
            
            transform.rotation = targetRotation;
        }
    }
}