using UnityEngine;

public class AnimalOverviewEntryButton : MonoBehaviour
{
    public GameObject animalObject;
    public Animal animalData;

    public CameraController cameraController;

    void Start()
    {
        
    }

    public void JumpToAnimal()
    {
        if (animalObject == null || cameraController == null)
            return;

        cameraController.JumpToAnimal(animalObject.transform);
    }
}
