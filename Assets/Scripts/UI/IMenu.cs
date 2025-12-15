using UnityEngine;

public interface IMenu
{
    void OpenInternal();
    void CloseInternal();
    RectTransform GetRect();
}