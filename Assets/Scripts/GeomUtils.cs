using UnityEngine;
using UnityEngine.EventSystems;

public static class GeomUtils
{
    public static Vector2 ClampToWindow(RectTransform parentCanvasRect, RectTransform windowRect,
        Vector2 pointerOffset, PointerEventData data)
    {
        Vector2 rawPointerPosition = data.position;

        Vector3[] canvasCorners = new Vector3[4];
        parentCanvasRect.GetWorldCorners(canvasCorners);

        float clampedX = Mathf.Clamp(rawPointerPosition.x, canvasCorners[0].x + pointerOffset.x,
            canvasCorners[2].x - (windowRect.rect.width - pointerOffset.x));

        float clampedY = Mathf.Clamp(rawPointerPosition.y, canvasCorners[0].y + (windowRect.rect.height + pointerOffset.y),
            canvasCorners[2].y + pointerOffset.y);


        Vector2 newPointerPosition = new Vector2(clampedX, clampedY);

        return newPointerPosition;
    }
}