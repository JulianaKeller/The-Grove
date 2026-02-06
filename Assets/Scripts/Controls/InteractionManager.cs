using DistantLands.Cozy;
using DistantLands.Cozy.Data;
using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    [Header("Water Source Creation")]

    private bool waterSourcePlacementActive;
    public float spawnHeightOffset = 1f;

    [Header("Weather Control")]
    public WeatherProfile weatherProfile;
    private bool weatherModeActive;

    [Header("Spawning")]
    public GameObject spawnIndicatorPrefab;
    public EntitySpeciesData speciesData;

    [SerializeField] private SimpleChanceEffector rainChanceEffector;

    private GameObject activeSpawnIndicator;
    private bool spawnModeActive = false;

    [Header("Selection & Focus")]

    public EntityView selectedAnimal;

    private CameraController cameraController;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && selectedAnimal != null)
        {
            cameraController.ToggleFocusMode(selectedAnimal.transform);
        }

        if (spawnModeActive || weatherModeActive || waterSourcePlacementActive)
        {
            HandleSpawnInput();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cameraController.ExitFocusModeRequest();
        }

        if (Input.GetMouseButtonDown(0))
            TrySelectAnimal();
    }

    private void CreateSpawnIndicator(bool clampToTerrain)
    {
        activeSpawnIndicator = Instantiate(spawnIndicatorPrefab);
        activeSpawnIndicator.SetActive(true);

        var follow = activeSpawnIndicator.GetComponent<FollowMouseAndClamp>();
        if (follow != null)
            follow.clampToTerrain = clampToTerrain;
    }

    private void RemoveSpawnIndicator()
    {
        if (activeSpawnIndicator != null)
        {
            activeSpawnIndicator.GetComponent<FollowMouseAndClamp>().enabled = false;
            var disappear = activeSpawnIndicator.GetComponent<IndicatorDisappear>();
            disappear.Disappear();
            activeSpawnIndicator = null;
        }
    }

    #region Water Source Creation

    public void ToggleWaterSourcePlacementMode(GameObject indicator)
    {
        SetSpawnIndicator(indicator);

        ExitAllModes();

        if (waterSourcePlacementActive)
        {
            return;
        }

        waterSourcePlacementActive = true;

        CreateSpawnIndicator(true);
        //ToDo scale water source spawn indicator to match baseRadius
    }

    private void TryCreateWaterSource()
    {
        if (InvalidSpawnLocation(WaterSourceManager.Instance.waterSourceRadius + WaterSourceManager.Instance.radiusVariation))
        {
            return;
        }

        Vector3 pos = activeSpawnIndicator.transform.position;

        int cellradius = WaterSourceManager.Instance.GetNewWaterSourceRadius();

        Debug.Log("Received a cellRadius of " + cellradius);

        if (!EnvironmentGrid.Instance.IsAreaInsideGrid(pos, cellradius))
            return;

        if (!ResourceManager.Instance.TryConsume(TokenType.CreateWaterSource))
            return;

        TerrainDeformer.Instance.LowerTerrain(pos, cellradius, irregularity: 0.15f, out var spawnHeight);

        WaterSourceManager.Instance.SpawnWaterSource(new Vector3(pos.x, spawnHeight - spawnHeightOffset, pos.z), cellradius);

        ExitWaterSourceCreationMode();
    }


    private void ExitWaterSourceCreationMode()
    {
        ExitAllModes();
    }

    #endregion

    #region WeatherControl

    public void ToggleSummonWeatherMode(WeatherProfile weather)
    {
        weatherProfile = weather;
        if (weatherModeActive)
            ExitWeatherMode();
        else
            EnterRainMode();
    }

    private void EnterRainMode()
    {
        if (!ResourceManager.Instance.Has(TokenType.ChangeWeather))
            return;

        ExitAllModes();

        weatherModeActive = true;

        CreateSpawnIndicator(false);
    }

    private void TrySetWeather()
    {
        if(weatherProfile == null)
        {
            Debug.Log("No weather profiles set...");
            return;
        }

        if (ResourceManager.Instance.TryConsume(TokenType.ChangeWeather))
        {
            CozyWeather.instance.weatherModule.ecosystem.SetWeather(weatherProfile, 15 * 10);
        }

        ExitWeatherMode();
    }

    private void ExitWeatherMode()
    {
        ExitAllModes();
    }

    #endregion

    #region Spawning

    private void HandleSpawnInput()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            ExitAllModes();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (spawnModeActive)
            {
                TrySpawnEntity();
            }
            if (weatherModeActive)
            {
                TrySetWeather();
            }
            if (waterSourcePlacementActive)
            {
                TryCreateWaterSource();
            }
            
        }
    }

    private void TrySpawnEntity()
    {
        if (InvalidSpawnLocation(0))
        {
            return;
        }

        TokenType tokenType =
        speciesData is AnimalSpeciesData
            ? TokenType.SpawnAnimal
            : TokenType.SpawnPlant;

        if (!ResourceManager.Instance.TryConsume(tokenType))
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

        ExitSpawnMode();
    }

    private void EnterSpawnMode()
    {
        if (spawnIndicatorPrefab == null)
            return;
        if (speciesData == null)
            return;

        ExitAllModes();

        spawnModeActive = true;

        CreateSpawnIndicator(true);
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
        ExitAllModes();
    }

    #endregion

    #region Selection

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

    #endregion

    #region Utility functions

    private void ExitAllModes()
    {
        spawnModeActive = false;
        weatherModeActive = false;
        waterSourcePlacementActive = false;

        RemoveSpawnIndicator();
    }

    private bool InvalidSpawnLocation(int checkRadius)
    {
        //Prevent spawning when mouse is over UI element
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return true;

        if (!activeSpawnIndicator.GetComponent<FollowMouseAndClamp>().IsOnTerrain)
            return true;

        // Check if spawn position is over a water source
        Vector3 spawnPos = activeSpawnIndicator.transform.position;
        List<WaterSource> nearbyWater = WorldManager.Instance.GetNearbyWaterSources(spawnPos, checkRadius);

        if (nearbyWater.Count > 0)
            return true;

        return false;
    }

    #endregion

}
