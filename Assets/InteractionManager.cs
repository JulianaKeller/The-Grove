using Unity.VisualScripting;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    [Header("Animal Spawning")]
    public GameObject spawnIndicatorPrefab;
    public AnimalSpeciesData chosenAnimal;

    private GameObject activeSpawnIndicator;
    private bool spawnModeActive = false;

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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cameraController.ExitFocusModeRequest();
        }

        if (spawnModeActive)
        {
            HandleSpawnInput();
        }
    }

    private void HandleSpawnInput()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            ExitAnimalSpawnMode();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            TrySpawnAnimal();
        }
    }

    private void TrySpawnAnimal()
    {
        //Prevent spawning when mouse is over UI element
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        if (!activeSpawnIndicator.GetComponent<FollowMouseAndClamp>().IsOnTerrain)
            return;

        Vector3 spawnPos = activeSpawnIndicator.transform.position;

        AnimalManager.Instance.SpawnAnimal(chosenAnimal, spawnPos);

        ExitAnimalSpawnMode();
    }

    private void EnterAnimalSpawnMode()
    {
        if (spawnIndicatorPrefab == null)
            return;
        if (chosenAnimal == null)
            return;

        CancelAllModes();

        spawnModeActive = true;

        activeSpawnIndicator = Instantiate(spawnIndicatorPrefab);
        activeSpawnIndicator.SetActive(true);
    }

    private void CancelAllModes()
    {
        // todo exit other spawning modes
    }

    public void ToggleAnimalSpawnMode()
    {
        //ToDo set spawn indicator
        if (!spawnModeActive)
        {
            EnterAnimalSpawnMode();
        }
        else
        {
            ExitAnimalSpawnMode();
        }
    }

    public void TogglePlantSpawnMode()
    {
        //ToDo set spawn indicator
        if (!spawnModeActive)
        {
            EnterAnimalSpawnMode();
        }
        else
        {
            ExitAnimalSpawnMode();
        }
    }

    private void ExitAnimalSpawnMode()
    {
        spawnModeActive = false;

        if (activeSpawnIndicator != null)
        {
            Destroy(activeSpawnIndicator);
            activeSpawnIndicator = null;
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

    private void ClearSelection()
    {
        if (selectedAnimal != null)
        {
            selectedAnimal.SetSelected(false);
            selectedAnimal = null;
        }
    }
}
