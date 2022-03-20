using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class MapController : MonoBehaviour, IDragHandler
{
    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Update()
    {
        this.transform.parent.GetComponent<RectTransform>().localScale *= Input.GetAxis("Mouse ScrollWheel") + 1f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //if (Input.GetKey(KeyCode.Space))
            rectTransform.anchoredPosition += (eventData.delta) / (this.transform.parent.GetComponent<RectTransform>().localScale.x) * 1.5f;
    }
}
