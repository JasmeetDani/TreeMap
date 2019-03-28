using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TreeMapCellSelectionHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TreeMapControllerCustomDraw controller;


    private List<PickableRect> rects = new List<PickableRect>();

    public List<PickableRect> Rects
    {
        get { return rects; }

        private set { }
    }


    private bool bMouseIn = false;


    public GameObject rectInternalNode;


    private GameObject selection;

    private RectTransform selectionRect;



    void Start()
    {
        selection = Instantiate(rectInternalNode, transform, false) as GameObject;

        selection.GetComponent<Image>().color = Color.white;


        selectionRect = selection.transform as RectTransform;


        selection.SetActive(false);
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

                    if (rt.gameObject == gameObject)
                    {
                        RectTransform rc = transform as RectTransform;

                        Vector2 screenPointLocal;

                        RectTransformUtility.ScreenPointToLocalPointInRectangle(rc, screenPoint, null, out screenPointLocal);

                        ShowSelection(screenPointLocal, rc.rect.width, rc.rect.height);

                        return;
                    }
                }
            }
        }
    }


    private void ShowSelection(Vector2 pos, float w, float h)
    {
        pos = new Vector2(pos.x / w, pos.y / h);

        for (int i = 0; i < rects.Count; ++i)
        {
            if ((pos.x >= rects[i].rect.anchorMin.x) && (pos.x <= rects[i].rect.anchorMax.x) &&
                (pos.y >= rects[i].rect.anchorMin.y) && (pos.y <= rects[i].rect.anchorMax.y))
            {
                selectionRect.anchorMin = rects[i].rect.anchorMin;

                selectionRect.anchorMax = rects[i].rect.anchorMax;


                // TODO : revisit for optimization
                selection.SetActive(true);


                controller.OnNeedToShowRecordInfo(rects[i].rowID);


                return;
            }
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        bMouseIn = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        bMouseIn = false;


        selection.SetActive(false);
    }
}