using UnityEngine;
using System.Collections;
using TMPro;

public class ControlUITrigger : MonoBehaviour
{
    [SerializeField] private GameObject messageText;
    [SerializeField] private float displayTime = 5f;
    [SerializeField] private float fadeDuration = 1f;

    [SerializeField] private GameObject[] objectsToDestroy;

    private bool hasTriggered = false;

    private void Start()
    {
        messageText.SetActive(false);
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

    /*
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
    }*/

    private IEnumerator ShowText()
    {
        messageText.SetActive(true);

        CanvasGroup canvasGroup = messageText.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = messageText.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(displayTime);

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        messageText.SetActive(false);

        foreach (GameObject obj in objectsToDestroy)
        {
            if (obj != null)
                Destroy(obj);
        }

        Destroy(gameObject);
    }
}
