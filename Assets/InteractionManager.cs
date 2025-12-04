using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    private AnimalView selectedAnimal;

    private CameraController cameraController;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TrySelectAnimal();

        if (Input.GetKeyDown(KeyCode.F) && selectedAnimal != null)
        {
            cameraController.ToggleFocusMode(selectedAnimal.transform);
        }

        if (cameraController.inFocusMode && Input.GetKeyDown(KeyCode.Escape))
        {
            cameraController.ExitFocusModeRequest();
        }
    }

    private void TrySelectAnimal()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f))
        {
            AnimalView animal = hit.collider.GetComponentInParent<AnimalView>();

            if (animal != null)
            {
                SelectAnimal(animal);
            }
            else
            {
                ClearSelection();
            }
        }
    }

    private void SelectAnimal(AnimalView animal)
    {
        if (selectedAnimal == animal)
            return;

        // Disable previous outline
        if (selectedAnimal != null)
            selectedAnimal.SetSelected(false);

        selectedAnimal = animal;
        selectedAnimal.SetSelected(true);
    }

    public void ClearSelection()
    {
        if (selectedAnimal != null)
        {
            selectedAnimal.SetSelected(false);
            selectedAnimal = null;
        }
    }
}
