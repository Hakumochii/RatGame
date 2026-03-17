using UnityEngine;

public class RotatingLoadingIcon : MonoBehaviour
{
    public float rotationSpeed = 50f; // grader per sekund

    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}
