using UnityEngine;
using UnityEngine.UI;

public class AnimalOverviewManager : MonoBehaviour
{
    public GameObject listEntry;

    public RectTransform menuRect;
    public ScrollRect scrollRect;
    public RectTransform contentRect;

    // private Map: Animal data -> button instance

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddEntry(GameObject animalObj, Animal data)
    {
        //Create AnimalOverviewEntryButton and add to list
    }

    public void RemoveEntry(Animal data)
    {

    }
}
