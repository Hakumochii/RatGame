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
        _rb.constraints = RigidbodyConstraints.FreezeAll;
        float moveSpeed = 2f;
        float rotateSpeed = 50f;

        while (Vector3.Distance(transform.position, target.position) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target.position,
                moveSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                target.rotation,
                rotateSpeed * Time.deltaTime
            );

            yield return null; // Wait one frame, then continue the loop
        }
    }
}
