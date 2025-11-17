using UnityEngine;

public class WaterSourceView : MonoBehaviour
{
    public WaterSource data;
    private Vector3 initialScale;
    public Vector3 currentScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        float fill = data.currentWater / data.capacity;
        currentScale = initialScale;
        currentScale.y = 0;
        transform.localScale = currentScale;
    }
}
