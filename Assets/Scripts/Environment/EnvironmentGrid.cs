using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class EnvironmentGrid
{
    public struct GridCell
    {
        public float temperature;
        public float moisture;
        public float fertility;
        public List<Animal> animals;
        public List<Plant> plants;
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
    public float cellSize = 1f;
    public Vector3 gridCenter = Vector3.zero;
    public GridCell[,] grid;

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
                    moisture = Random.Range(0.4f, 0.8f),
                    fertility = Random.Range(0.3f, 0.9f),
                    animals = new List<Animal>(),
                    plants = new List<Plant>()
                };
            }
        }
    }

    public Vector2Int GetCellCoords(Vector3 position)
    {
        int x = Mathf.Clamp(
            Mathf.FloorToInt((position.x + gridSize * 0.5f) / cellSize),
            0, gridSize - 1);
        int z = Mathf.Clamp(
            Mathf.FloorToInt((position.z + gridSize * 0.5f) / cellSize),
            0, gridSize - 1);
        return new Vector2Int(x, z);
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

    public void UpdateGrid(float dt)
    {
        //e.g. update fertility
    }
}
