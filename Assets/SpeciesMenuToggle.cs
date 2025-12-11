using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpeciesMenuToggle : MonoBehaviour
{
    public enum SpeciesType { Animal, Plant }

    public SpeciesType type;
    public GameObject buttonPrefab; //use this prefab for the species buttons
    public GameObject menuBox;
    
    public Vector2 menuBounds = new Vector2(300, 400); //if the species buttons don't fit inside the box, add a scroll bar and make it scrollable

    private bool menuOpen = false;
    
    private RectTransform menuRect;
    private ScrollRect scrollRect;
    private RectTransform contentRect;
    private GameObject clickBlocker;

    private float animationDuration = 0.25f;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClicked);

        menuRect = menuBox.GetComponent<RectTransform>();
        scrollRect = menuBox.GetComponent<ScrollRect>();
        contentRect = scrollRect.content;

        menuBox.SetActive(false);
        menuRect.sizeDelta = new Vector2(menuBounds.x, 0f);

        clickBlocker = transform.parent.Find("MenuBlocker").gameObject;
        clickBlocker.SetActive(false);
        clickBlocker.GetComponent<Button>().onClick.AddListener(CloseMenu);
    }

    public void OnClicked()
    {
        if (menuOpen)
            StartCoroutine(CloseMenuCoroutine());
        else
            StartCoroutine(OpenMenuCoroutine());
    }

    public void CloseMenu()
    {
        StartCoroutine(CloseMenuCoroutine());
    }

    IEnumerator OpenMenuCoroutine()
    {
        menuOpen = true;
        menuBox.SetActive(true);
        clickBlocker.SetActive(true);

        menuRect.sizeDelta = new Vector2(menuBounds.x, 0f);
        yield return StartCoroutine(AnimateHeight(0, menuBounds.y));

        yield return PopulateMenu();
    }

    IEnumerator CloseMenuCoroutine()
    {
        ClearMenu();
        yield return StartCoroutine(AnimateHeight(menuBounds.y, 0));

        menuBox.SetActive(false);
        clickBlocker.SetActive(false);
        menuOpen = false;
    }

    IEnumerator AnimateHeight(float start, float end)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / animationDuration;
            float k = Mathf.SmoothStep(0f, 1f, t);
            float h = Mathf.Lerp(start, end, k);

            menuRect.sizeDelta = new Vector2(menuBounds.x, h);
            yield return null;
        }
    }

    void ClearMenu()
    {
        for (int i = contentRect.childCount - 1; i >= 0; i--)
            Destroy(contentRect.GetChild(i).gameObject);
    }

    IEnumerator PopulateMenu()
    {
        string folder = type == SpeciesType.Animal
        ? "ScriptableObjects/AnimalSpecies"
        : "ScriptableObjects/PlantSpecies";

        var allSpecies = Resources.LoadAll<EntitySpeciesData>(folder);

        foreach (var data in allSpecies)
        {
            Debug.Log("Loaded Species: " + data.ToString());
            GameObject button = Instantiate(buttonPrefab, contentRect);
            var sb = button.GetComponent<SpeciesButton>();
            sb.AssignSpecies(data);
        }

        scrollRect.vertical = contentRect.sizeDelta.y > menuBounds.y;

        yield return null;
    }
}
