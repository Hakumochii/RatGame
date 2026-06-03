using UnityEngine;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{
    public void ShowText(GameObject go)
    {
        StartCoroutine(FadeOutObject(go));
    }

    private IEnumerator FadeOutObject(GameObject go)
    {
        go.SetActive(true);

        CanvasGroup canvasGroup = go.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = go.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(5f);

        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        go.SetActive(false);
    }

    /*
    private IEnumerator FadeOutText(TextMeshProUGUI text)
    {
        text.gameObject.SetActive(true);

        Color color = text.color;
        color.a = 1f;
        text.color = color;

        yield return new WaitForSeconds(5f);

        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            color.a = Mathf.Lerp(1f, 0f, elapsed / duration);
            text.color = color;

            yield return null;
        }

        text.gameObject.SetActive(false);
    }*/
}
