using UnityEngine;
using UnityEngine.EventSystems;

public class MouseDownHandler : MonoBehaviour, IPointerClickHandler
{
    public delegate void OnMouseDown(Vector2 pos);

    public event OnMouseDown MouseDown;


    public void OnPointerClick(PointerEventData eventData)
    {
        MouseDown((transform as RectTransform).position);
    }
}