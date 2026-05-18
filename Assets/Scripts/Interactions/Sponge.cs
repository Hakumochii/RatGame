using UnityEngine;
using System.Collections;

public class Sponge : MonoBehaviour
{
    public Transform target;

    private Rigidbody _rb;
    

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            StartCoroutine(MoveSponge());
        }
    }

    IEnumerator MoveSponge()
    {
        _rb.useGravity = false;
        float speed = 5f;
        this.transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        this.transform.rotation = Quaternion.RotateTowards(transform.rotation, target.rotation, speed * Time.deltaTime);
        yield return null;
        
    }
}
