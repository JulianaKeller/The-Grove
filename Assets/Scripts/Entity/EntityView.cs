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
        GetComponent<EntityOutline>()?.SetOutlineVisible(selected);
    }
}
