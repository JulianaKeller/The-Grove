using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;

[System.Serializable]

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    [Header("Spawning")]
    public GameObject spawnIndicatorPrefab;
    public EntitySpeciesData speciesData;

    private GameObject activeSpawnIndicator;
    private bool spawnModeActive = false;

    private EntityView selectedAnimal;

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
            ExitSpawnMode();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            TrySpawnEntity();
        }
    }

    private void TrySpawnEntity()
    {
        //Prevent spawning when mouse is over UI element
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;

        if (!activeSpawnIndicator.GetComponent<FollowMouseAndClamp>().IsOnTerrain)
            return;

        Vector3 spawnPos = activeSpawnIndicator.transform.position;

        EntitySpeciesData data = speciesData;

        if (data is AnimalSpeciesData animalData)
        {
            AnimalManager.Instance.SpawnAnimal(animalData, spawnPos);
        }
        else if (data is PlantSpeciesData plantData)
        {
            PlantManager.Instance.SpawnPlant(plantData, spawnPos);
        }

        if (activeSpawnIndicator != null)
        {
            activeSpawnIndicator.GetComponent<FollowMouseAndClamp>().enabled = false;
            var disappear = activeSpawnIndicator.GetComponent<IndicatorDisappear>();
            disappear.Disappear();
            activeSpawnIndicator = null;
        }

        ExitSpawnMode();
    }

    private void EnterSpawnMode()
    {
        if (spawnIndicatorPrefab == null)
            return;
        if (speciesData == null)
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

    public void ToggleSpawnMode(EntitySpeciesData data)
    {
        speciesData = data;

        if (!spawnModeActive)
        {
            EnterSpawnMode();
        }
        else
        {
            ExitSpawnMode();
        }
    }

    public void SetSpawnIndicator(GameObject spawnIndicator)
    {
        spawnIndicatorPrefab = spawnIndicator;
    }

    private void ExitSpawnMode()
    {
        spawnModeActive = false;
        speciesData = null;

        if (activeSpawnIndicator != null)
        {
            Destroy(activeSpawnIndicator);
            activeSpawnIndicator = null;
        }
    }

    //------------ Selection --------------------------------------

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
