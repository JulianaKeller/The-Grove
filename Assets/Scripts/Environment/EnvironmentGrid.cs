using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class EnvironmentGrid
{
    public struct GridCell
    {
        public float temperature;
        public float moisture; //0-1
        public float fertility; //0-1
        public List<Animal> animals;
        public List<Plant> plants;
        public List<WaterSource> waterSources;
    }

    //This script stores environment data (moisture, temperature, vegetation density)
    //ToDo 

    private static EnvironmentGrid _instance;
    public static EnvironmentGrid Instance
    {
        get
        {
            if (_instance == null)
                _instance = new EnvironmentGrid();
            return _instance;
        }
    }

    public int gridSize = 100;
    public int cellSize = 10;
    public Vector3 gridCenter = Vector3.zero;
    public GridCell[,] grid;

    [Header("Regeneration/usage constants")]
    public float fertilityRegenRate = 0.0005f;  // fertility gained per update
    public float moistureLossRate = 0.00005f;   // moisture gained per update
    public float minFertility = 0f;
    public float maxFertility = 1f;
    public float minMoisture = 0f;
    public float maxMoisture = 1f;
    public float lightRainMoistureBonus = 0.0001f; // moisture gained per update during light rain
    public float heavyRainMoistureBonus = 0.0005f; // moisture gained per update during heavy rain

    private EnvironmentGrid()
    {
        grid = new GridCell[gridSize, gridSize];

        // Initialize grid with default values
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                grid[x, z] = new GridCell
                {
                    temperature = Random.Range(15f, 25f),
                    moisture = maxMoisture,
                    fertility = maxFertility,
                    animals = new List<Animal>(),
                    plants = new List<Plant>(),
                    waterSources = new List<WaterSource>()
                };
            }
        }
    }

    public Vector2Int GetCellCoords(Vector3 position)
    {
        int x = Mathf.Clamp(
            Mathf.FloorToInt((position.x + (gridSize * cellSize * 0.5f)) / cellSize),
            0, gridSize - 1);
        int z = Mathf.Clamp(
            Mathf.FloorToInt((position.z + (gridSize * cellSize * 0.5f)) / cellSize),
            0, gridSize - 1);
        return new Vector2Int(x, z);
    }

    public bool IsAreaInsideGrid(Vector3 worldPos, float worldRadius)
    {
        float halfSize = gridSize * cellSize * 0.5f;

        float minX = gridCenter.x - halfSize;
        float maxX = gridCenter.x + halfSize;
        float minZ = gridCenter.z - halfSize;
        float maxZ = gridCenter.z + halfSize;

        if (worldPos.x - worldRadius < minX) return false;
        if (worldPos.x + worldRadius > maxX) return false;
        if (worldPos.z - worldRadius < minZ) return false;
        if (worldPos.z + worldRadius > maxZ) return false;

        return true;
    }

    public bool IsAreaInsideGrid(Vector3 worldPos, int cellRadius)
    {
        Vector2Int centerCell = EnvironmentGrid.Instance.GetCellCoords(worldPos);

        if (centerCell.x - cellRadius < 0) return false;
        if (centerCell.y - cellRadius < 0) return false;
        if (centerCell.x + cellRadius >= gridSize) return false;
        if (centerCell.y + cellRadius >= gridSize) return false;

        return true;
    }

    public void RegisterAnimal(Animal a)
    {
        var coords = GetCellCoords(a.position);
        grid[coords.x, coords.y].animals.Add(a);
    }

    public void DeregisterAnimal(Animal a)
    {
        var coords = GetCellCoords(a.position);
        grid[coords.x, coords.y].animals.Remove(a);
    }

    public void RegisterPlant(Plant p)
    {
        var coords = GetCellCoords(p.position);
        grid[coords.x, coords.y].plants.Add(p);
    }

    public void DeregisterPlant(Plant p)
    {
        var coords = GetCellCoords(p.position);
        grid[coords.x, coords.y].plants.Remove(p);
    }

    public void RegisterWaterSource(WaterSource s)
    {
        Vector2Int centerCoords = GetCellCoords(s.center);

        int minX = Mathf.Max(centerCoords.x - s.cellRadius, 0);
        int maxX = Mathf.Min(centerCoords.x + s.cellRadius, gridSize - 1);
        int minZ = Mathf.Max(centerCoords.y - s.cellRadius, 0);
        int maxZ = Mathf.Min(centerCoords.y + s.cellRadius, gridSize - 1);

        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                GridCell cell = grid[x, z];

                if (!cell.waterSources.Contains(s))
                    cell.waterSources.Add(s);

                // --- Remove animals in this cell ---
                if (cell.animals != null)
                {
                    List<Animal> animalsCopy = new List<Animal>(cell.animals);
                    foreach (Animal a in animalsCopy)
                    {
                        if (a != null)
                            AnimalManager.Instance.RemoveAnimal(a);
                    }
                }

                // --- Remove plants in this cell ---
                if (cell.plants != null)
                {
                    List<Plant> plantsCopy = new List<Plant>(cell.plants);
                    foreach (Plant p in plantsCopy)
                    {
                        if (p != null)
                            PlantManager.Instance.RemovePlant(p);
                    }
                }

                grid[x, z] = cell;
            }
        }
    }

    public void DeregisterWaterSource(WaterSource s)
    {
        Vector2Int centerCoords = GetCellCoords(s.center);

        int minX = Mathf.Max(centerCoords.x - s.cellRadius, 0);
        int maxX = Mathf.Min(centerCoords.x + s.cellRadius, gridSize - 1);
        int minZ = Mathf.Max(centerCoords.y - s.cellRadius, 0);
        int maxZ = Mathf.Min(centerCoords.y + s.cellRadius, gridSize - 1);

        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                grid[x, z].waterSources.Remove(s);
            }
        }
    }

    private string GetPath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    public void PrintGridAnimals()
    {
        string csv = ConvertGridToCsv(cell => cell.animals.Count.ToString());
        WriteCsv("Grid_Animals.csv", csv);
    }

    public void PrintGridPlants()
    {
        string csv = ConvertGridToCsv(cell => cell.plants.Count.ToString());
        WriteCsv("Grid_Plants.csv", csv);
    }

    public void PrintGridWaterSources()
    {
        string csv = ConvertGridToCsv(cell => cell.waterSources.Count.ToString());
        WriteCsv("Grid_Water_Sources.csv", csv);
    }

    public void PrintGridMoisture()
    {
        string csv = ConvertGridToCsv(cell => cell.moisture.ToString("F3"));
        WriteCsv("Grid_Moisture.csv", csv);
    }

    public void PrintGridTemperature()
    {
        string csv = ConvertGridToCsv(cell => cell.temperature.ToString("F3"));
        WriteCsv("Grid_Temperature.csv", csv);
    }

    public void PrintGridFertility()
    {
        string csv = ConvertGridToCsv(cell => cell.fertility.ToString("F3"));
        WriteCsv("Grid_Fertility.csv", csv);
    }

    private void WriteCsv(string fileName, string content)
    {
        string path = GetPath(fileName);
        File.WriteAllText(path, content, Encoding.UTF8);
        Debug.Log($"CSV exported: {path}");
    }

    private string ConvertGridToCsv(System.Func<GridCell, string> cellValue)
    {
        StringBuilder sb = new StringBuilder();

        for (int z = 0; z < gridSize; z++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                sb.Append(cellValue(grid[x, z]));
                if (x < gridSize - 1)
                    sb.Append(";");
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    public void UpdateGrid(float timeStep)
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int z = 0; z < gridSize; z++)
            {
                GridCell cell = grid[x, z];

                foreach(WaterSource s in cell.waterSources)
                {
                    ApplyWaterInfluence(s, x, z, timeStep);
                }

                if (WaterSourceManager.Instance.lightRain)
                {
                    cell.moisture = Mathf.Clamp(cell.moisture + lightRainMoistureBonus, minMoisture, maxMoisture);
                }
                else if (WaterSourceManager.Instance.heavyRain || WaterSourceManager.Instance.thunderstorm)
                {
                    cell.moisture = Mathf.Clamp(cell.moisture + heavyRainMoistureBonus, minMoisture, maxMoisture);
                }

                float fertilityLoss = 0f;

                foreach (Plant p in cell.plants)
                {
                    float maturity = Mathf.Clamp01(p.age / p.species.lifespan);

                    fertilityLoss += p.species.groundFertilityUsage * maturity;
                }

                // fertility decreases due to plants
                cell.fertility = Mathf.Clamp(
                    cell.fertility - fertilityLoss,
                    minFertility, maxFertility
                );

                // moisture decreases due to plants - done in Plant as moisture draw differs by plant needs

                // natural moisture loss
                cell.moisture = Mathf.Clamp(cell.moisture - moistureLossRate, minMoisture, maxMoisture);

                // natural fertility regeneration
                cell.fertility = Mathf.Clamp(cell.fertility + fertilityRegenRate, minFertility, maxFertility);

                grid[x, z] = cell;
            }
        }
    }

    private void ApplyWaterInfluence(WaterSource ws, int cellX, int cellZ, float timeStep) //x and z are the indices of the cell in which the water source is contained
    {
        int cells = Mathf.CeilToInt(WaterSourceManager.Instance.influenceRadius / cellSize);

        int startX = Mathf.Max(cellX - cells, 0);
        int endX = Mathf.Min(cellX + cells, gridSize - 1);
        int startZ = Mathf.Max(cellZ - cells, 0);
        int endZ = Mathf.Min(cellZ + cells, gridSize - 1);

        for (int x = startX; x <= endX; x++)
        {
            for (int z = startZ; z <= endZ; z++)
            {
                GridCell cell = grid[x, z];

                float moistureBonus = WaterSourceManager.Instance.moistureBonus * timeStep * (ws.currentWater / ws.capacity);
                float fertilityBonus = WaterSourceManager.Instance.fertilityBonus * timeStep * (ws.currentWater / ws.capacity);

                cell.moisture = Mathf.Clamp(cell.moisture + moistureBonus, minMoisture, maxMoisture);
                cell.fertility = Mathf.Clamp(cell.fertility + fertilityBonus, minFertility, maxFertility);

                grid[x, z] = cell;
            }
        }
    }
}
