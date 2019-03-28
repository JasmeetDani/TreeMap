using UnityEngine;
using UnityEngine.EventSystems;

public class ResizePanel : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public Vector2 minSize;
    public Vector2 maxSize;

    private RectTransform canvasRectTransform;

    private RectTransform rectTransform;

    private Vector2 currentPointerPosition;
    private Vector2 previousPointerPosition;


    public RectTransform ClientRect = null;


    private Vector2 sizeDelta;


    public delegate void OnMouseDown();

    public event OnMouseDown MouseDown;

    public delegate void OnResize();

    public event OnResize Resize;


    void Awake()
    {
        // Every floating window will be added to a Canvas

        Canvas canvas = GetComponentInParent<Canvas>();

        canvasRectTransform = canvas.transform as RectTransform;


        rectTransform = transform.parent.GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData data)
    {
        // rectTransform.SetAsLastSibling();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, data.position, data.pressEventCamera, out previousPointerPosition);
        
        if (MouseDown != null)
            MouseDown();
    }

    public void OnDrag(PointerEventData data)
    {
        Vector2 pointerPostion = GeomUtils.ClampToWindow(canvasRectTransform, rectTransform, previousPointerPosition, data);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, pointerPostion, data.pressEventCamera, out currentPointerPosition);
        Vector2 resizeValue = currentPointerPosition - previousPointerPosition;

        sizeDelta = rectTransform.sizeDelta;

        sizeDelta += new Vector2(resizeValue.x, -resizeValue.y);
        sizeDelta = new Vector2(
            Mathf.Clamp(sizeDelta.x, minSize.x, maxSize.x),
            Mathf.Clamp(sizeDelta.y, minSize.y, maxSize.y)
        );

        rectTransform.sizeDelta = sizeDelta;


        previousPointerPosition = currentPointerPosition;
    }

    public void OnPointerUp(PointerEventData data)
    {
        if (Resize != null)
            Resize();
    }
}