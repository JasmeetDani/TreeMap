using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class TreeMapMouseHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TreeMapControllerCustomDraw controller;


    private Image graphic;

    private Color origColor;

    public GameObject container;

    private bool bMouseIn = false;


    void Start()
    {
        graphic = container.GetComponent<Image>();


        origColor = graphic.color;
    }

    void Update()
    {
        if (bMouseIn)
        {
            // TODO : revisit for optimisation

            Vector2 screenPoint = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            PointerEventData pointerData = new PointerEventData(EventSystem.current);

            pointerData.position = screenPoint;

            List<RaycastResult> results = new List<RaycastResult>();

            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                foreach (RaycastResult rt in results)
                {
                    if (rt.gameObject.GetComponent<TreeMapCellSelectionHandler>() != null)
                    {

                        return;
                    }

                    if (rt.gameObject.GetComponent<TreeMapMouseHoverHandler>() != null)
                    {
                        if (rt.gameObject == gameObject)
                        {
                            controller.OnNeedToShowFrameInfo(GetComponent<CustomData>().ID);
                        }

                        return;
                    }
                }
            }
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        bMouseIn = true;

        graphic.color = Color.white;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        bMouseIn = false;

        graphic.color = origColor;
    }
}