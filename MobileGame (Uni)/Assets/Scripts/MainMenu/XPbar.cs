using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XPbar : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 50f;
    [SerializeField] private float resetPosition = 500f;
    [SerializeField] private float startPosition = 0f;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        rectTransform.anchoredPosition += Vector2.right * scrollSpeed * Time.deltaTime;

        if (rectTransform.anchoredPosition.x >= resetPosition)
        {
            Vector2 position = rectTransform.anchoredPosition;
            position.x = startPosition;
            rectTransform.anchoredPosition = position;
        }
    }
}
