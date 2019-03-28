using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class TreeMapViewCustomDraw : MonoBehaviour, IScrollHandler
{
    public TreeMapControllerCustomDraw controller;


    public RectTransform content;

    public Text footer;

    public GameObject rectInternalNode;

    public GameObject rectInternalNodeAtLowestLevel;


    private Dictionary<string, RectWithParentTransform> cellRects = new Dictionary<string, RectWithParentTransform>();


    private float totalWeight = 0;

    private List<int> SkippedDraw = new List<int>();
    private List<UIRectangle> uiRects = new List<UIRectangle>();


    private int Rejected = 0;

    private int Drawn = 0;


    private RectTransform TreeMapContainerRect = null;


    public bool Invert;


    private const int BGIndex = 0;

    private const int ContentIndex = 1;

    private const int HeaderIndex = 0;

    private const int ClientAreaIndex = 1;


    private Stack<int> zoomLayer = new Stack<int>();


    private int ZoomScale = 1;

    private const int maxZoomScale = 20;

    private Action onZoomInZoomOut;


    public void DrawHeirarchy(TreeMapNode root)
    {
        zoomLayer.Push(root.ID);
        
        totalWeight = root.Data.SizeByWeight;
        
        RefreshTreeMap(root);
    }


    private bool RejectNodeInternal(RectTransform r)
    {
        if ((r.rect.xMin > r.rect.xMax) || (r.rect.yMin > r.rect.yMax))
        {
            return true;
        }

        return false;
    }

    private bool RejectNode(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        RectTransform rt = parent.transform as RectTransform;

        float width = rt.rect.width * (anchorMax.x - anchorMin.x);
        float height = rt.rect.height * (anchorMax.y - anchorMin.y);

        if ((width < 1) && (height < 1))
            return true;

        return false;
    }

    private GameObject DrawSelf(TreeMapNode r, Transform parent, Vector2 anchorMin, Vector2 anchorMax, int level)
    {
        GameObject ret;

        if (r.Data.Value == null)
        {
            ret = DrawInternalNode(r, this.rectInternalNode, parent, anchorMin, anchorMax, 0, 0, 0, 0, 0);

            ret.gameObject.tag = "Internal";


            ret.transform.GetChild(BGIndex).gameObject.SetActive(false);

            ret.transform.GetChild(ContentIndex).GetChild(HeaderIndex).gameObject.SetActive(false);


            RectTransform rt = ret.transform.GetChild(ContentIndex).GetChild(ClientAreaIndex) as RectTransform;

            rt.anchorMin = Vector2.zero;

            rt.anchorMax = new Vector2(1, 1);


            // Assume client rect as second child
            ret = ret.transform.GetChild(ContentIndex).GetChild(ClientAreaIndex).gameObject;

            return ret;
        }


        float offsetXMin = Constants.minPadding;

        float offsetYMin = Constants.minPadding;

        float offsetXMax = Constants.minPadding;

        float offsetYMax = Constants.minPadding;

        float headerPercentage = Constants.defaultHeaderPercentage;


        RectTransform rc = parent.transform as RectTransform;

        float width = rc.rect.width * (anchorMax.x - anchorMin.x);

        float height = rc.rect.height * (anchorMax.y - anchorMin.y);

        PutInHeaderAndPadding(width, height, ref offsetXMin, ref offsetYMin, ref offsetXMax, ref offsetYMax,
            ref headerPercentage);


        if (r.Children[0].Children.Count > 0)
        {
            ret = DrawInternalNode(r, this.rectInternalNode, parent, anchorMin, anchorMax,
                offsetXMin, offsetYMin, offsetXMax, offsetYMax, headerPercentage);

            ret.gameObject.tag = "Internal";

            ret.gameObject.GetComponent<TreeMapMouseHoverHandler>().controller = controller;


            FrameMouseDownHandler temp = ret.GetComponent<FrameMouseDownHandler>();

            temp.LeftMouseDown += ZoomIn;

            temp.RightMouseDown += ZoomOut;


            // Assume header as first child

            ret.transform.GetChild(ContentIndex).GetChild(HeaderIndex).GetComponent<Text>().text = r.Data.Value.ToString() + " " +
            ((r.Data.SizeByWeight / totalWeight) * 100).ToString("F2") + "%";


            float BGcolor = (Constants.headerStartColor.r * 255 - (level + 1) * Constants.headerColorSubtractor) / 255.0f;

            if (BGcolor < 0)
                BGcolor = 0;

            ret.transform.GetComponent<Image>().color = new Color(BGcolor, BGcolor, BGcolor);

            ret.transform.GetChild(BGIndex).GetComponent<Image>().color = new Color(BGcolor, BGcolor, BGcolor);


            if (RejectNodeInternal(ret.transform as RectTransform))
            {
                SkippedDraw.Add(r.ID);

                ++Rejected;
            }
        }
        else
        {
            ret = DrawInternalNode(r, this.rectInternalNodeAtLowestLevel, parent, anchorMin, anchorMax,
                offsetXMin, offsetYMin, offsetXMax, offsetYMax, headerPercentage);

            ret.gameObject.GetComponent<TreeMapMouseHoverHandler>().controller = controller;

            ret.transform.GetChild(ContentIndex).GetChild(ClientAreaIndex).GetComponent<TreeMapCellSelectionHandler>().controller = controller;


            FrameMouseDownHandler temp = ret.GetComponent<FrameMouseDownHandler>();

            temp.LeftMouseDown += ZoomIn;

            temp.RightMouseDown += ZoomOut;


            // Assume header as first child
            ret.transform.GetChild(ContentIndex).GetChild(HeaderIndex).GetComponent<Text>().text = r.Data.Value.ToString() + " " +
            ((r.Data.SizeByWeight / totalWeight) * 100).ToString("F2") + "%";


            float BGcolor = (Constants.headerStartColor.r * 255 - (level + 1) * Constants.headerColorSubtractor) / 255.0f;

            if (BGcolor < 0)
                BGcolor = 0;

            ret.transform.GetComponent<Image>().color = new Color(BGcolor, BGcolor, BGcolor);

            ret.transform.GetChild(BGIndex).GetComponent<Image>().color = new Color(BGcolor, BGcolor, BGcolor);


            if (RejectNodeInternal(ret.transform as RectTransform))
            {
                SkippedDraw.Add(r.ID);

                ++Rejected;
            }
        }


        // Assume client rect as second child

        ret = ret.transform.GetChild(ContentIndex).GetChild(ClientAreaIndex).gameObject;

        this.uiRects.Add(ret.GetComponent<UIRectangle>());

        return ret;
    }

    private void DrawChildren(TreeMapNode r, Transform parent, Vector2 anchorMin, Vector2 anchorMax, int level)
    {
        UIRectangle rc = parent.GetComponent<UIRectangle>();

        Vector2 newAnchorMin = anchorMin;

        Vector2 newAnchorMax = anchorMax;

        if (Invert)
        {
            // Invert the anchors

            newAnchorMin = new Vector2(anchorMin.x, 1 - anchorMax.y);

            newAnchorMax = new Vector2(anchorMax.x, 1 - anchorMin.y);
        }


        PickableRect rect = new PickableRect()
        {
            rect = new CustomRect { anchorMin = newAnchorMin, anchorMax = newAnchorMax },

            color = controller.LookUp(r.Data.ColorByWeight),

            rowID = r.Data.rowID
        };

        parent.GetComponent<TreeMapCellSelectionHandler>().Rects.Add(rect);

        rc.AddCellToDraw(rect);

        cellRects.Add(r.Data.rowID, new RectWithParentTransform()
        {
            rect = rect.rect,

            parent = parent as RectTransform
        });


        if (RejectNode(parent, anchorMin, anchorMax))
        {
            SkippedDraw.Add(r.ID);

            ++Rejected;
        }
        else
        {
            ++Drawn;
        }
    }


    private bool DrawVertically(float width, float height)
    {
        return width > height;
    }

    private void Squarify(TreeMapNode r, Transform parent, float topW, float topH, Vector2 anchorMin, Vector2 anchorMax, int level)
    {
        if (r.Children.Count == 0)
        {
            DrawChildren(r, parent, anchorMin, anchorMax, level);

            return;
        }


        float w = topW, h = topH;


        float area = w * h;


        // We do not handle negative weights

        float total_value = Math.Abs(r.Data.SizeByWeight);

        float VA_ratio = area / total_value;


        bool drawV = DrawVertically(w, h);

        float shorter_side = drawV ? h : w;


        TreeMapNode[] children = r.Children.ToArray();

        Array.Sort(children, (x, y) => {

            int ret = x.Data.SizeByWeight.CompareTo(y.Data.SizeByWeight);

            ret = ret * -1;

            return ret;
        });

        CustomRect[] rects = new CustomRect[children.Length];

        for (int i = 0; i < rects.Length; ++i)
        {
            rects[i] = new CustomRect();
        }


        float current_ratio = float.MaxValue;

        float sum_before_freeze = 0;


        int count = children.Length, rectsFrozenBeforeIndex = 0;

        float startXTemp = 0, startYTemp = 0;

        float startX = 0, startY = 0, endX = 0, endY = 0;

        for (int i = 0; i < count;)
        {
            float c1 = Math.Abs(children[i].Data.SizeByWeight) * VA_ratio;

            sum_before_freeze = sum_before_freeze + c1;


            float other_side = sum_before_freeze / shorter_side;

            float new_shorter_side = c1 / other_side;

            float ratio_c1 = Math.Max(other_side / new_shorter_side, new_shorter_side / other_side);


            if (ratio_c1 < current_ratio)
            {
                current_ratio = ratio_c1;

                // Position / Reposition and scale all the rects

                startXTemp = startX;

                startYTemp = startY;

                if (drawV)
                {
                    endX = other_side / topW;

                    endY = 0;

                    for (int j = rectsFrozenBeforeIndex; j <= i; ++j)
                    {
                        float c1Temp = Math.Abs(children[j].Data.SizeByWeight) * VA_ratio;

                        float other_sideTemp = sum_before_freeze / shorter_side;

                        endY = c1Temp / other_sideTemp / topH;


                        rects[j].anchorMin = new Vector2(startXTemp, startYTemp);
                        rects[j].anchorMax = new Vector2(startXTemp + endX, startYTemp + endY);


                        startYTemp = startYTemp + endY;
                    }
                }
                else
                {
                    endY = other_side / topH;

                    endX = 0;

                    for (int j = rectsFrozenBeforeIndex; j <= i; ++j)
                    {
                        float c1Temp = Math.Abs(children[j].Data.SizeByWeight) * VA_ratio;

                        float other_sideTemp = sum_before_freeze / shorter_side;

                        endX = c1Temp / other_sideTemp / topW;


                        rects[j].anchorMin = new Vector2(startXTemp, startYTemp);
                        rects[j].anchorMax = new Vector2(startXTemp + endX, startYTemp + endY);


                        startXTemp = startXTemp + endX;
                    }
                }

                // Process next only when the current has found its place

                i++;
            }
            else
            {
                // Start afresh


                rectsFrozenBeforeIndex = i;


                // Recalculate w and h, very important

                if (drawV)
                {
                    startX = startXTemp + endX;

                    w = topW * (1 - startX);
                }
                else
                {
                    startY = startYTemp + endY;

                    h = topH * (1 - startY);
                }

                // We do not need to set area, total_value and VA_ratio

                drawV = DrawVertically(w, h);

                shorter_side = drawV ? h : w;

                current_ratio = float.MaxValue;

                sum_before_freeze = 0;
            }
        }


        // Create the root game object

        GameObject newObj = DrawSelf(r, parent, anchorMin, anchorMax, level);

        RectTransform rt = newObj.transform as RectTransform;

        // We are done with self, now let us recurse for every child

        for (int i = 0; i < count; ++i)
        {
            float wFinal = (rects[i].anchorMax.x - rects[i].anchorMin.x) * rt.rect.width;

            float hFinal = (rects[i].anchorMax.y - rects[i].anchorMin.y) * rt.rect.height;

            Squarify(children[i], newObj.transform, wFinal, hFinal, rects[i].anchorMin, rects[i].anchorMax, level + 1);
        }
    }


    private GameObject DrawInternalNode(TreeMapNode r, GameObject obj, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
                                        float offsetXMin, float offsetYMin, float offsetXMax, float offsetYMax, float headerPercentage)
    {
        GameObject newObj = Instantiate(obj, parent, false) as GameObject;

        newObj.GetComponent<CustomData>().ID = r.ID;


        Vector2 newAnchorMin = anchorMin;

        Vector2 newAnchorMax = anchorMax;

        if (Invert)
        {
            // Invert the anchors

            newAnchorMin = new Vector2(anchorMin.x, 1 - anchorMax.y);

            newAnchorMax = new Vector2(anchorMax.x, 1 - anchorMin.y);
        }


        RectTransform rc = newObj.transform as RectTransform;

        rc.anchorMin = newAnchorMin;
        rc.anchorMax = newAnchorMax;


        // Set offsets on container

        rc = newObj.transform.GetChild(ContentIndex) as RectTransform;

        rc.offsetMin = new Vector2(offsetXMin, offsetYMin);

        rc.offsetMax = new Vector2(-offsetXMax, 0);


        RectTransform rtHeader = newObj.transform.GetChild(ContentIndex).GetChild(HeaderIndex).transform as RectTransform;

        RectTransform rtClientArea = newObj.transform.GetChild(ContentIndex).GetChild(ClientAreaIndex).transform as RectTransform;


        rtHeader.anchorMin = new Vector2(rtHeader.anchorMin.x, 1 - headerPercentage);

        rtHeader.anchorMax = new Vector2(rtHeader.anchorMax.x, 1);


        rtClientArea.anchorMin = new Vector2(rtClientArea.anchorMin.x, 0);

        rtClientArea.anchorMax = new Vector2(rtClientArea.anchorMax.x, 1 - headerPercentage);


        return newObj;
    }


    private void ReCreateTreeMap(TreeMapNode root)
    {
        // Destroy the current TreeMap View and reset the bookkeeping

        DestroyImmediate(content.GetChild(0).gameObject);

        cellRects.Clear();


        RefreshTreeMap(root);
    }

    private void RefreshTreeMap(TreeMapNode root)
    {
        Drawn = 0;

        Rejected = 0;


        TreeMapContainerRect = content as RectTransform;

        TreeMapContainerRect.anchorMax = new Vector2(1, 1);

        TreeMapContainerRect.anchorMin = Vector2.zero;

        TreeMapContainerRect.offsetMin = Vector2.zero;

        TreeMapContainerRect.offsetMax = Vector2.zero;

        this.uiRects.Clear();

        Squarify(root, content, TreeMapContainerRect.rect.width, TreeMapContainerRect.rect.height, new Vector2(0, 0), new Vector2(1, 1), 1);

        for (int i = 0; i < this.uiRects.Count; i++)
        {
            if (uiRects[i] != null)
            {
                this.uiRects[i].Invalidate();
            }
        }


        // Debug.Log("Drawn " + Drawn);

        // Debug.Log("Rejected " + Rejected);
    }


    private void PutInHeaderAndPadding(float origWidth, float origHeight, ref float offsetXMin, ref float offsetYMin,
                                       ref float offsetXMax, ref float offsetYMax, ref float headerPercentage)
    {
        var reductionAreaEachLevelPer = 0.85f;

        float c4, c3, c2, c1;

        float f1 = 0.25f;
        float f2 = 0.25f;
        float T;          // Top percentage - we define this so that the label rendering can happen

        // Now that the factors have been defined
        c1 = origWidth * origHeight;
        c2 = (-1 * origWidth) - (origWidth * f2) - (f1 * origHeight) - (f2 * origHeight);
        c3 = f1 + f1 * f2 + f2 + f2 * f2;
        c4 = reductionAreaEachLevelPer * origWidth * origHeight;

        float numberToSqrt = numberToSqrt = ((c4 - c1) / c3) + Mathf.Pow((c2 / (c3 * 2)), 2);

        if (numberToSqrt < 0)
        {
            // Error
            return;
        }

        float rootOfNumber = rootOfNumber = Mathf.Sqrt(numberToSqrt);

        // So we have to possible solutions

        float firstPart = -1 * c2 / (c3 * 2);

        float solution1;
        float solution2;

        solution1 = firstPart + rootOfNumber;
        solution2 = firstPart - rootOfNumber;

        // Now we need to find the most appropriate number
        if ((solution1 < 0) || (solution2 < 0))
        {   
            // One or both values are negative
            if ((solution1 < 0) && (solution2 > 0))
            {
                // I've left out the equal sign as this would be silly to have this solution
                T = solution2;

            }
            else if ((solution1 > 0) && (solution2 < 0))
            {
                T = solution1;
            }
            else
            {
                // error
                return;
                // Mathematically this equation should be solvable with a real solution
            }
        }
        else
        {
            // Both are positive
            // Take the value that is closest to the _model.nodeHeaderPer
            if (Mathf.Abs(0.1f - (solution1 / origHeight)) < Math.Abs(0.1f - (solution2 / origHeight)))
            {
                T = solution1;
            }
            else
            {
                T = solution2;
            }
        }

        // Calculated result is in absolute values

        headerPercentage = T / origHeight;

        offsetYMax = T;
        if (offsetYMax < Constants.minPadding)
        {
            offsetYMax = Constants.minPadding;
        }

        offsetXMin = T * f1;
        if (offsetXMin < Constants.minPadding)
        {
            offsetXMin = Constants.minPadding;
        }

        offsetXMax = T * f2;
        if (offsetXMax < Constants.minPadding)
        {
            offsetXMax = Constants.minPadding;
        }

        offsetYMin = T * f2;
        if (offsetYMin < Constants.minPadding)
        {
            offsetYMin = Constants.minPadding;
        }
    }


    public void SetZoomInZoomOutListner(Action onZoomInZoomOut)
    {
        this.onZoomInZoomOut = onZoomInZoomOut;
    }

    public void ResetZoom()
    {
        ZoomScale = 1;

        zoomLayer.Clear();
    }


    public void ZoomIn(int ID)
    {
        if (ID == 0)
        {
            // We cannot zoom in on the root

            return;
        }

        zoomLayer.Push(ID);

        ReCreateTreeMap(controller.LookUp(ID));

        RefreshFooter();

        InvokeEvent();
    }

    public void ZoomOut(int ID)
    {
        int topID = zoomLayer.Peek();

        if (topID == 0)
        {
            // We are already zoomed out fully

            return;
        }

        if (topID == ID)
        {
            zoomLayer.Pop();

            int current = zoomLayer.Peek();

            ReCreateTreeMap(controller.LookUp(current));

            RefreshFooter();

            InvokeEvent();
        }
    }

    private void InvokeEvent()
    {
        if (onZoomInZoomOut != null)
        {
            onZoomInZoomOut();
        }
    }


    public void OnScroll(PointerEventData data)
    {
        if (data.scrollDelta.y > 0)
        {
            ZoomInSimple();
        }
        else
        {
            ZoomOutSimple();
        }
    }


    public void ZoomInSimple()
    {
        if (TreeMapContainerRect != null)
        {
            if (ZoomScale + 1 <= maxZoomScale)
            {
                ZoomScale = ZoomScale + 1;


                TreeMapContainerRect.anchorMax = TreeMapContainerRect.anchorMax + new Vector2(0.3f, 0.3f);

                TreeMapContainerRect.anchorMin = TreeMapContainerRect.anchorMin - new Vector2(0.3f, 0.3f);
            }
        }
    }

    public void ZoomOutSimple()
    {
        if (TreeMapContainerRect != null)
        {
            if (ZoomScale > 1)
            {
                ZoomScale = ZoomScale - 1;

                TreeMapContainerRect.anchorMax = TreeMapContainerRect.anchorMax - new Vector2(0.3f, 0.3f);

                TreeMapContainerRect.anchorMin = TreeMapContainerRect.anchorMin + new Vector2(0.3f, 0.3f);
            }
        }
    }


    private void RefreshFooter()
    {
        TreeMapNode node = controller.LookUp(zoomLayer.Peek());

        string message = string.Empty;

        while (node.Parent != null)
        {
            message = " " + node.Data.Value.ToString() + " \\" + message;

            node = node.Parent;
        }
        
        footer.text = message;
    }
}