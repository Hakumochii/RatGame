using UnityEngine;

public class CarFreeze : MonoBehaviour
{
    public GameObject car;
    void OnTriggerEnter(Collider other)
    {
        if (CompareTag("Player"))
        {
            Rigidbody rb = car.GetComponent<Rigidbody>();
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true; 
            
        }
        
    }
}
