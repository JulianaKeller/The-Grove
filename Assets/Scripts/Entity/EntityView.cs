using UnityEngine;

public class EntityView : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SetSelected(bool selected)
    {
        /*var outline = GetComponent<AnimalOutline>();
        if (outline != null)
        {
            outline.SetVisible(selected);
        }*/

        GetComponent<AnimalOutline>().SetOutlineVisible(selected);
    }
}
