using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimalOverviewManager : MonoBehaviour
{
    public GameObject listEntry;

    public RectTransform menuRect;
    public ScrollRect scrollRect;
    public RectTransform contentRect;

    public CameraController cameraController;

    private Dictionary<Animal, GameObject> entries = new Dictionary<Animal, GameObject>();

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void AddEntry(GameObject animalObj, Animal data)
    {
        if (entries.ContainsKey(data))
            return;

        GameObject entry = Instantiate(listEntry, contentRect);

        AnimalOverviewEntryButton button = entry.GetComponent<AnimalOverviewEntryButton>();

        button.animalObject = animalObj;
        button.animalData = data;
        button.cameraController = cameraController;

        entries.Add(data, entry);
    }

    public void RemoveEntry(Animal data)
    {
        if (!entries.ContainsKey(data))
            return;

        GameObject entry = entries[data];
        entries.Remove(data);

        Destroy(entry);
    }
}
