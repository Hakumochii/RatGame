using UnityEngine;

public class OutlineController : MonoBehaviour
{
    [SerializeField] private Outline outline;

    private void Start()
    {
        outline.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            outline.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            outline.enabled = false;
        }
    }
}
