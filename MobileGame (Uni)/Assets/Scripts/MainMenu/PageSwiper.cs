using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class PageSwiper : MonoBehaviour, IDragHandler, IEndDragHandler
{
    private Vector3 panelLocation;
    public float swipeThresh = 0.2f;
    public float easing = 0.5f;

    public int currentPage = 2;
    public int leftPage = 1;
    public int rightPage = 3;

    public Image PageNavigator;


    // Start is called before the first frame update
    void Start()
    {
        panelLocation = transform.position;
    }

    public void OnDrag(PointerEventData data)
    {
        float difference = data.pressPosition.x - data.position.x;
        transform.position = panelLocation - new Vector3(difference, 0, 0);
    }

    public void OnEndDrag(PointerEventData data)
    {
        float percentage = (data.pressPosition.x - data.position.x) / Screen.width;
        if(Mathf.Abs(percentage) >= swipeThresh)
        {
            Vector3 newLocation = panelLocation;
            // swiping right
            if(percentage > 0 && currentPage < 3)
            {
                currentPage++;
                newLocation += new Vector3(-Screen.width, 0, 0);
            }
            else if (percentage < 0 && currentPage > 1)
            {
                currentPage--;
                newLocation += new Vector3(Screen.width, 0, 0);
            }

            StartCoroutine(SmoothSwipe(transform.position, newLocation, easing));
            panelLocation = newLocation;
        }
        else
        {
            StartCoroutine(SmoothSwipe(transform.position, panelLocation, easing));
        }
    }

   IEnumerator SmoothSwipe(Vector3 startPos, Vector3 endPos, float seconds)
    {
        float t = 0f;
        while (t <= 1.0f)
        {
            t += Time.deltaTime / seconds;
            transform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));

            yield return null;
        }
    }

    public void ButtonTapLeft()
    {
        if(currentPage > leftPage)
        {
            currentPage--;
            Vector3 newLocation = panelLocation + new Vector3(Screen.width, 0, 0);
            StartCoroutine(SmoothSwipe(transform.position, newLocation, easing));
            panelLocation = newLocation;
        }
    }


    public void ButtonTapRight()
    {
        if (currentPage < rightPage)
        {
            currentPage++;
            Vector3 newLocation = panelLocation + new Vector3(-Screen.width, 0, 0);
            StartCoroutine(SmoothSwipe(transform.position, newLocation, easing));
            panelLocation = newLocation;
        }
    }
}
