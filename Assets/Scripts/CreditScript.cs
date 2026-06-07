using UnityEngine;
using UnityEngine.UI;

public class CreditScript : MonoBehaviour
{
    public float scrollspeed = 55f;

    private RectTransform rectTransform;


    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        rectTransform.anchoredPosition += Vector2.up * scrollspeed * Time.deltaTime;
    }
}
