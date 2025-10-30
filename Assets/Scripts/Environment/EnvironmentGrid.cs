using UnityEngine;

public class EnvironmentGrid
{
    public struct GridCell
    {
        public float temperature;
        public float moisture;
        public float fertility;
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

    private EnvironmentGrid()
    {
        
    }

    public void UpdateGrid(float dt)
    {
        
    }
}
