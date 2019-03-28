using UnityEngine;
using UnityEngine.EventSystems;

public class FrameMouseDownHandler : MonoBehaviour, IPointerClickHandler
{
    public delegate void OnLeftMouseDown(int ID);

    public event OnLeftMouseDown LeftMouseDown;


    public delegate void OnRightMouseDown(int ID);

    public event OnRightMouseDown RightMouseDown;


    public void OnPointerClick(PointerEventData eventData)
    {

        int ID = GetComponent<CustomData>().ID;

        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                LeftMouseDown(ID);
                break;

            case PointerEventData.InputButton.Right:
                RightMouseDown(ID);
                break;
        }
    }
}