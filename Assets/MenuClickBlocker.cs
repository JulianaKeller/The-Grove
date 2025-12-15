using UnityEngine;
using UnityEngine.EventSystems;

public class MenuClickBlocker : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        MenuManager.Instance.CloseAll();
    }
}