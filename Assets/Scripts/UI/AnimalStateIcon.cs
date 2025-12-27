using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Animal;

public class AnimalStateIcon : MonoBehaviour
{
    [System.Serializable]
    public struct StateIcon
    {
        public AnimalVisualState state;
        public Sprite icon;
    }

    public Image iconImage;
    public Image bubble;
    public Canvas canvas;
    public List<StateIcon> icons;

    private Dictionary<AnimalVisualState, Sprite> iconMap;

    private Animal animal;

    void Awake()
    {
        iconImage = GetComponent<Image>();
        iconMap = new Dictionary<AnimalVisualState, Sprite>();
        foreach (var entry in icons)
        {
            iconMap[entry.state] = entry.icon;
            //Debug.Log("New sprite dictionary entry: State " + entry.state + " has icon " + (iconMap[entry.state]? iconMap[entry.state].name:iconMap[entry.state]));
        }
    }

    public void Initialize(Animal animal)
    {
        this.animal = animal;
        animal.OnStateChanged.AddListener(UpdateIcon);
    }

    void OnDisable()
    {
        if (animal != null)
            animal.OnStateChanged.RemoveListener(UpdateIcon);
    }

    private void UpdateIcon(AnimalVisualState state)
    {
        bool success = iconMap.TryGetValue(state, out var currentIcon);
        //Debug.Log("State " + state + " has icon " + (currentIcon ? currentIcon.name : currentIcon));
        //Debug.Log("State " + state + " has icon " + (iconMap[state] != null ? iconMap[state].name : iconMap[state]));
        if (!success || currentIcon == null)
        {
            DisableThoughtBubble();
            //Debug.Log("Disabled thought bubble due to " + (!success ? "failed TryGetValue" : "Sprite: " + currentIcon));
            return;
        }

        EnableThoughtBubble();
        iconImage.sprite = currentIcon;
    }

    private void DisableThoughtBubble()
    {
        iconImage.enabled = false;
        bubble.enabled = false;
        canvas.enabled = false;
    }

    private void EnableThoughtBubble()
    {
        canvas.enabled = true;
        bubble.enabled = true;
        iconImage.enabled = true;
    }
}
