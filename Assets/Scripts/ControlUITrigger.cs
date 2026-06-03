using UnityEngine;
using System.Collections;
using TMPro;

public class ControlUITrigger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float displayTime = 5f;

    [SerializeField] private GameObject[] objectsToDestroy;

    private bool hasTriggered = false;

    private void Start()
    {
        messageText.gameObject.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (hasTriggered)
            return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(ShowText());
        }
    }

    private IEnumerator ShowText()
    {
        messageText.gameObject.SetActive(true);

        yield return new WaitForSeconds(displayTime);

        messageText.gameObject.SetActive(false);

        foreach (GameObject obj in objectsToDestroy)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        Destroy(gameObject);
    }
}
