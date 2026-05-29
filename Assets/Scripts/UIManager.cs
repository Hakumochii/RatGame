using UnityEngine;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{
    public void ShowText(TextMeshProUGUI text)
    {
        StartCoroutine(FadeOutText(text));
    }

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
    }
}
